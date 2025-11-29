'use client'

import { useState } from 'react'
import { useMutation, useQuery } from '@apollo/client'
import { CREATE_PATIENT, UPDATE_PATIENT, GET_PATIENT, ADD_DIAGNOSTIC_RESULT } from '../../lib/queries'

interface Patient {
  id?: string
  firstName: string
  lastName: string
  dateOfBirth: string
}

interface PatientFormProps {
  patient?: Patient | null
  onSuccess: () => void
  onCancel: () => void
  // Switch the form from editing existing patient to adding a new one
  onResetToAdd?: () => void
}

export default function PatientForm({ patient, onSuccess, onCancel, onResetToAdd }: PatientFormProps) {
  const normalizeDate = (value: string | undefined): string => {
    if (!value) return ''
    // If already in YYYY-MM-DD, keep it
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value
    // Try to parse other date strings
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return ''
    const yyyy = d.getFullYear()
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    return `${yyyy}-${mm}-${dd}`
  }

  // Helper to get today's date in YYYY-MM-DD for <input type="date"> max
  const todayStr = (): string => {
    const d = new Date()
    const yyyy = d.getFullYear()
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    return `${yyyy}-${mm}-${dd}`
  }

  const [formData, setFormData] = useState<Patient>({
    firstName: patient?.firstName || '',
    lastName: patient?.lastName || '',
    dateOfBirth: normalizeDate(patient?.dateOfBirth),
  })

  const [createPatient, { loading: creating }] = useMutation(CREATE_PATIENT, {
    onCompleted: onSuccess,
    refetchQueries: ['GetPatients']
  })

  const [updatePatient, { loading: updating }] = useMutation(UPDATE_PATIENT, {
    onCompleted: onSuccess,
    refetchQueries: ['GetPatients']
  })

  const loading = creating || updating

  const [dateError, setDateError] = useState<string>('')

  // Robust future-date validation using calendar (UTC) comparison
  const parseYmdToUtc = (ymd: string): number | null => {
    // expects YYYY-MM-DD
    const m = ymd.match(/^(\d{4})-(\d{2})-(\d{2})$/)
    if (!m) return null
    const y = Number(m[1])
    const mo = Number(m[2]) - 1
    const d = Number(m[3])
    return Date.UTC(y, mo, d)
  }

  const isDateInFuture = (ymd: string): boolean => {
    const ts = parseYmdToUtc(ymd)
    if (ts == null) return false
    const now = new Date()
    const todayUtc = Date.UTC(now.getFullYear(), now.getMonth(), now.getDate())
    return ts > todayUtc
  }

  // Load detailed patient with diagnostic results when editing existing patient
  const {
    data: patientDetailData,
    loading: loadingPatientDetail,
    error: patientDetailError,
    refetch: refetchPatientDetail,
  } = useQuery(GET_PATIENT, {
    skip: !patient?.id,
    variables: { id: patient?.id },
    fetchPolicy: 'cache-and-network',
  })

  const [addDiagnosisForm, setAddDiagnosisForm] = useState({ diagnosis: '', notes: '' })
  const [addDiagnosticResult, { loading: addingDiagnosis }] = useMutation(ADD_DIAGNOSTIC_RESULT, {
    onCompleted: async () => {
      setAddDiagnosisForm({ diagnosis: '', notes: '' })
      // Refresh the patient diagnostic results list
      if (patient?.id) await refetchPatientDetail()
    },
  })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    // Validate date is not in the future
    if (formData.dateOfBirth) {
      const dob = formData.dateOfBirth
      if (isDateInFuture(dob)) {
        setDateError('Date of birth cannot be in the future.')
        return
      }
    }
    
    if (patient?.id) {
      await updatePatient({
        variables: {
          command: {
            patientId: patient.id,
            firstName: formData.firstName,
            lastName: formData.lastName,
            dateOfBirth: formData.dateOfBirth
          }
        }
      })
    } else {
      await createPatient({
        variables: {
          command: {
            firstName: formData.firstName,
            lastName: formData.lastName,
            dateOfBirth: formData.dateOfBirth
          }
        }
      })
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    // If changing date, validate against future date
    if (name === 'dateOfBirth') {
      if (value && isDateInFuture(value)) {
        setDateError('Date of birth cannot be in the future.')
      } else {
        setDateError('')
      }
    }

    setFormData({
      ...formData,
      [name]: value
    })
  }

  const handleResetToAdd = () => {
    // Clear local form state
    setFormData({ firstName: '', lastName: '', dateOfBirth: '' })
    // Notify parent to drop selected patient but keep form view
    if (onResetToAdd) onResetToAdd()
  }

  return (
    <div className="patient-form">
      <h2 className="text-xl font-semibold mb-6">
        {patient?.id ? 'Edit Patient' : 'Add New Patient'}
      </h2>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label htmlFor="firstName" className="form-label">First Name</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
              className="form-input"
            />
          </div>
          <div>
            <label htmlFor="lastName" className="form-label">Last Name</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
              className="form-input"
            />
          </div>
        </div>


        <div>
          <label htmlFor="dateOfBirth" className="form-label">Date of Birth</label>
          <input
            type="date"
            id="dateOfBirth"
            name="dateOfBirth"
            value={formData.dateOfBirth}
            onChange={handleChange}
            max={todayStr()}
            required
            className="form-input"
          />
          {dateError && (
            <p className="text-sm text-red-600 mt-1">{dateError}</p>
          )}
        </div>

        <div className="flex gap-4 pt-4">
          <button
            type="submit"
            disabled={loading || !!dateError}
            className="btn-primary disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? 'Saving...' : (patient?.id ? 'Update Patient' : 'Add Patient')}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="btn-secondary"
          >
            Cancel
          </button>
          {patient?.id && (
            <button
              type="button"
              onClick={handleResetToAdd}
              className="btn-secondary"
            >
              Reset
            </button>
          )}
        </div>
      </form>

      {patient?.id && (
        <div className="mt-10">
          <h3 className="text-lg font-semibold mb-4">Diagnoses</h3>
          {loadingPatientDetail && (
            <div className="text-gray-600">Loading diagnoses...</div>
          )}
          {patientDetailError && (
            <div className="text-red-600">Failed to load diagnoses: {patientDetailError.message}</div>
          )}

          {/* List of diagnostic results */}
          <div className="space-y-3">
            {(patientDetailData?.patient?.diagnosticResults || []).map((dr: any) => (
              <div key={dr.id} className="border rounded p-3 bg-white">
                <div className="flex justify-between">
                  <div className="font-medium">{dr.diagnosis}</div>
                  <div className="text-sm text-gray-500">{new Date(dr.timestampUtc).toLocaleString()}</div>
                </div>
                {dr.notes && <div className="text-gray-700 mt-1 whitespace-pre-wrap">{dr.notes}</div>}
              </div>
            ))}
            {(!patientDetailData?.patient?.diagnosticResults || patientDetailData.patient.diagnosticResults.length === 0) && !loadingPatientDetail && (
              <div className="text-gray-500">No diagnoses yet.</div>
            )}
          </div>

          {/* Add new diagnosis */}
          <div className="mt-6">
            <h4 className="font-semibold mb-2">Add Diagnosis</h4>
            <div className="grid grid-cols-1 gap-3">
              <div>
                <label className="form-label" htmlFor="diagnosis">Diagnosis</label>
                <input
                  id="diagnosis"
                  type="text"
                  className="form-input"
                  value={addDiagnosisForm.diagnosis}
                  onChange={(e) => setAddDiagnosisForm({ ...addDiagnosisForm, diagnosis: e.target.value })}
                  placeholder="Enter diagnosis"
                />
              </div>
              <div>
                <label className="form-label" htmlFor="notes">Notes</label>
                <textarea
                  id="notes"
                  className="form-input"
                  value={addDiagnosisForm.notes}
                  onChange={(e) => setAddDiagnosisForm({ ...addDiagnosisForm, notes: e.target.value })}
                  placeholder="Optional notes"
                />
              </div>
              <div>
                <button
                  type="button"
                  className="btn-primary disabled:bg-gray-400 disabled:cursor-not-allowed"
                  disabled={!addDiagnosisForm.diagnosis || addingDiagnosis}
                  onClick={async () => {
                    if (!patient?.id) return
                    await addDiagnosticResult({
                      variables: {
                        input: {
                          patientId: patient.id,
                          diagnosis: addDiagnosisForm.diagnosis,
                          notes: addDiagnosisForm.notes || null,
                        },
                      },
                    })
                  }}
                >
                  {addingDiagnosis ? 'Adding...' : 'Add Diagnosis'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
