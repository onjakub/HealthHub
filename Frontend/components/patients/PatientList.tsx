'use client'

import { useState } from 'react'
import { useQuery } from '@apollo/client'
import { GET_PATIENTS } from '../../lib/queries'

interface Patient {
  id: string
  firstName: string
  lastName: string
  dateOfBirth: string
  createdAt: string
}

interface PatientListProps {
  onEditPatient: (patient: Patient) => void
}

export default function PatientList({ onEditPatient }: PatientListProps) {
  const { loading, error, data } = useQuery(GET_PATIENTS)
  const [searchTerm, setSearchTerm] = useState('')

  if (loading) return <div className="text-center py-8">Loading patients...</div>
  if (error) return <div className="text-center py-8 text-red-600">Error loading patients: {error.message}</div>

  const patients = data?.patients?.nodes || []
  
  const filteredPatients = patients.filter((patient: Patient) =>
    `${patient.firstName} ${patient.lastName}`.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <div>
      <div className="mb-6">
        <input
          type="text"
          placeholder="Search patients by name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="grid gap-4">
        {filteredPatients.map((patient: Patient) => (
          <div key={patient.id} className="patient-card">
            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">{patient.firstName} {patient.lastName}</h3>
                <p className="text-gray-600">
                  Date of Birth: {new Date(patient.dateOfBirth).toLocaleDateString()}
                </p>
              </div>
              <button
                onClick={() => onEditPatient(patient)}
                className="btn-primary"
              >
                Edit
              </button>
            </div>
          </div>
        ))}
        
        {filteredPatients.length === 0 && (
          <div className="text-center py-8 text-gray-500">
            {searchTerm ? 'No patients found matching your search.' : 'No patients found.'}
          </div>
        )}
      </div>
    </div>
  )
}
