import { gql } from '@apollo/client'

export const GET_PATIENTS = gql`
  query GetPatients {
    patients {
      items {
        id
        firstName
        lastName
        dateOfBirth
        email
        phoneNumber
        address
        createdAt
        updatedAt
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
      }
    }
  }
`

export const GET_PATIENT = gql`
  query GetPatient($id: UUID!) {
    patient(id: $id) {
      id
      firstName
      lastName
      dateOfBirth
      email
      phoneNumber
      address
      createdAt
      updatedAt
    }
  }
`

export const GET_PATIENT_DIAGNOSTIC_RESULTS = gql`
  query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
    patientDiagnosticResults(patientId: $patientId, limit: $limit) {
      id
      patientId
      diagnosis
      description
      date
      severity
      createdAt
    }
  }
`

export const CREATE_PATIENT = gql`
  mutation CreatePatient($command: CreatePatientCommandInput!) {
    createPatient(command: $command) {
      id
      firstName
      lastName
      dateOfBirth
    }
  }
`

export const UPDATE_PATIENT = gql`
  mutation UpdatePatient($command: UpdatePatientCommandInput!) {
    updatePatient(command: $command) {
      id
      firstName
      lastName
      dateOfBirth
    }
  }
`

export const DELETE_PATIENT = gql`
  mutation DeletePatient($command: DeletePatientCommandInput!) {
    deletePatient(command: $command)
  }
`

export const ADD_DIAGNOSTIC_RESULT = gql`
  mutation AddDiagnosticResult($input: AddDiagnosticResultInput!) {
    addDiagnosticResult(input: $input) {
      id
      patientId
      diagnosis
      description
      date
      severity
    }
  }
`
