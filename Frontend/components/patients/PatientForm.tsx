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
      {/* Header */}
      <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
          <h2 className="text-2xl font-bold text-gray-900">
            {patient?.id ? 'Edit Patient' : 'Add New Patient'}
          </h2>
        </div>
        {patient?.id && (
          <span className="badge-active">Editing</span>
        )}
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div>
            <label htmlFor="firstName" className="form-label">
              <span className="flex items-center gap-2">
                <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                First Name
              </span>
            </label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
              className="form-input"
              placeholder="John"
            />
          </div>
          <div>
            <label htmlFor="lastName" className="form-label">
              <span className="flex items-center gap-2">
                <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                Last Name
              </span>
            </label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
              className="form-input"
              placeholder="Doe"
            />
          </div>
        </div>

        <div>
          <label htmlFor="dateOfBirth" className="form-label">
            <span className="flex items-center gap-2">
              <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              Date of Birth
            </span>
          </label>
          <div className="relative">
            <input
              type="date"
              id="dateOfBirth"
              name="dateOfBirth"
              value={formData.dateOfBirth}
              onChange={handleChange}
              max={todayStr()}
              required
              className="form-input pr-8"
            />
            <div className="absolute inset-y-0 right-2 flex items-center pointer-events-none">
            </div>
          </div>
          {dateError && (
            <div className="flex items-center gap-2 mt-2 text-red-600">
              <svg className="w-4 h-4 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
              <p className="text-sm font-medium">{dateError}</p>
            </div>
          )}
        </div>

        {/* Action buttons */}
        <div className="flex flex-wrap gap-3 pt-4 border-t border-gray-200">
          <button
            type="submit"
            disabled={loading || !!dateError}
            className="btn-primary flex items-center gap-2"
          >
            {loading ? (
              <>
                <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span>Saving...</span>
              </>
            ) : (
              <>
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
                <span>{patient?.id ? 'Update Patient' : 'Add Patient'}</span>
              </>
            )}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="btn-secondary flex items-center gap-2"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            <span>Cancel</span>
          </button>
          {patient?.id && (
            <button
              type="button"
              onClick={handleResetToAdd}
              className="btn-secondary flex items-center gap-2"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              <span>Add New</span>
            </button>
          )}
        </div>
      </form>

      {patient?.id && (
        <div className="mt-10 pt-8 border-t-2 border-gray-200">
          {/* Diagnoses section header */}
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h3 className="text-xl font-bold text-gray-900">Medical Diagnoses</h3>
          </div>

          {loadingPatientDetail && (
            <div className="flex items-center gap-3 py-4 text-gray-600">
              <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>Loading diagnoses...</span>
            </div>
          )}

          {patientDetailError && (
            <div className="alert-error flex items-start gap-3">
              <svg className="w-5 h-5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <div>
                <p className="font-semibold">Failed to load diagnoses</p>
                <p className="text-sm mt-1">{patientDetailError.message}</p>
              </div>
            </div>
          )}

          {/* List of diagnostic results */}
          <div className="space-y-3 mb-8">
            {(patientDetailData?.patient?.diagnosticResults || []).map((dr: any) => (
              <div key={dr.id} className="bg-white border border-gray-200 rounded-xl p-5 hover:shadow-md transition-shadow">
                <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3 mb-3">
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center flex-shrink-0 mt-0.5">
                      <svg className="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div>
                      <h4 className="font-semibold text-gray-900 text-base">{dr.diagnosis}</h4>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 text-sm text-gray-500">
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <span>{new Date(dr.timestampUtc).toLocaleString()}</span>
                  </div>
                </div>
                {dr.notes && (
                  <div className="ml-11 text-gray-700 text-sm bg-gray-50 rounded-lg p-3 whitespace-pre-wrap">
                    {dr.notes}
                  </div>
                )}
              </div>
            ))}
            {(!patientDetailData?.patient?.diagnosticResults || patientDetailData.patient.diagnosticResults.length === 0) && !loadingPatientDetail && (
              <div className="text-center py-8">
                <div className="inline-flex items-center justify-center w-12 h-12 bg-gray-100 rounded-full mb-3">
                  <svg className="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </div>
                <p className="text-gray-600">No diagnoses recorded yet.</p>
              </div>
            )}
          </div>

          {/* Add new diagnosis form */}
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
            <div className="flex items-center gap-2 mb-4">
              <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              <h4 className="font-semibold text-gray-900">Add New Diagnosis</h4>
            </div>
            <div className="space-y-4">
              <div>
                <label className="form-label" htmlFor="diagnosis">
                  <span className="flex items-center gap-2">
                    <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                    Diagnosis
                  </span>
                </label>
                <input
                  id="diagnosis"
                  type="text"
                  className="form-input"
                  value={addDiagnosisForm.diagnosis}
                  onChange={(e) => setAddDiagnosisForm({ ...addDiagnosisForm, diagnosis: e.target.value })}
                  placeholder="Enter diagnosis name"
                />
              </div>
              <div>
                <label className="form-label" htmlFor="notes">
                  <span className="flex items-center gap-2">
                    <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Clinical Notes
                  </span>
                </label>
                <textarea
                  id="notes"
                  rows={3}
                  className="form-input resize-none"
                  value={addDiagnosisForm.notes}
                  onChange={(e) => setAddDiagnosisForm({ ...addDiagnosisForm, notes: e.target.value })}
                  placeholder="Optional clinical notes and observations"
                />
              </div>
              <div>
                <button
                  type="button"
                  className="btn-success flex items-center gap-2"
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
                  {addingDiagnosis ? (
                    <>
                      <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      <span>Adding...</span>
                    </>
                  ) : (
                    <>
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                      <span>Add Diagnosis</span>
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
