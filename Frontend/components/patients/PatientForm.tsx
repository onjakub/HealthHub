'use client'

import { useState } from 'react'
import { useMutation } from '@apollo/client'
import { CREATE_PATIENT, UPDATE_PATIENT } from '../../lib/queries'

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
}

export default function PatientForm({ patient, onSuccess, onCancel }: PatientFormProps) {
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
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
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    })
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
            required
            className="form-input"
          />
        </div>

        <div className="flex gap-4 pt-4">
          <button
            type="submit"
            disabled={loading}
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
        </div>
      </form>
    </div>
  )
}
