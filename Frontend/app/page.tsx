'use client'

import { useState, useEffect } from 'react'
import { ApolloProvider } from '@apollo/client'
import client from '../lib/graphql-client'
import PatientList from '../components/patients/PatientList'
import PatientForm from '../components/patients/PatientForm'
import Login from '../components/auth/Login'

export default function Home() {
  const [currentView, setCurrentView] = useState<'list' | 'form'>('list')
  const [selectedPatient, setSelectedPatient] = useState<any>(null)
  const [isAuthenticated, setIsAuthenticated] = useState(false)

  useEffect(() => {
    const token = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null
    if (token) setIsAuthenticated(true)
  }, [])

  const handleLogin = (token: string) => {
    localStorage.setItem('authToken', token)
    setIsAuthenticated(true)
  }

  const handleLogout = () => {
    localStorage.removeItem('authToken')
    setIsAuthenticated(false)
  }

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} />
  }

  return (
    <ApolloProvider client={client}>
      <div className="min-h-screen bg-gray-50">
        <header className="bg-white shadow-sm border-b">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
            <div className="flex justify-between items-center">
              <h1 className="text-2xl font-bold text-gray-900">HealthHub Patient Management</h1>
              <button
                onClick={handleLogout}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
              >
                Logout
              </button>
            </div>
            <nav className="mt-2">
              <button
                onClick={() => setCurrentView('list')}
                className={`mr-4 px-3 py-2 rounded-md text-sm font-medium ${
                  currentView === 'list'
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                Patient List
              </button>
              <button
                onClick={() => {
                  // Ensure the form opens in "add new" mode
                  setSelectedPatient(null)
                  setCurrentView('form')
                }}
                className={`px-3 py-2 rounded-md text-sm font-medium ${
                  currentView === 'form'
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                Add Patient
              </button>
            </nav>
          </div>
        </header>

        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {currentView === 'list' && (
            <PatientList
              onEditPatient={(patient) => {
                setSelectedPatient(patient)
                setCurrentView('form')
              }}
            />
          )}
          {currentView === 'form' && (
            <PatientForm
              patient={selectedPatient}
              onSuccess={() => {
                setSelectedPatient(null)
                setCurrentView('list')
              }}
              onCancel={() => {
                setSelectedPatient(null)
                setCurrentView('list')
              }}
              onResetToAdd={() => {
                // Switch the form into "add new" mode without leaving the form view
                setSelectedPatient(null)
                setCurrentView('form')
              }}
            />
          )}
        </main>
      </div>
    </ApolloProvider>
  )
}
