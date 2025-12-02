import { gql } from '@apollo/client'

export const GET_PATIENTS = gql`
  query GetPatients {
    patients {
      nodes {
        id
        firstName
        lastName
        fullName
        age
        lastDiagnosis
        createdAt
        dateOfBirth
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
      fullName
      dateOfBirth
      age
      lastDiagnosis
      createdAt
      diagnosticResults {
        id
        diagnosis
        notes
        timestampUtc
      }
    }
  }
`

export const GET_PATIENT_DIAGNOSTIC_RESULTS = gql`
  query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
    patientDiagnosticResults(patientId: $patientId, limit: $limit) {
      id
      diagnosis
      notes
      timestampUtc
    }
  }
`

export const CREATE_PATIENT = gql`
  mutation CreatePatient($command: CreatePatientCommandInput!) {
    createPatient(command: $command) {
      id
      firstName
      lastName
      fullName
      dateOfBirth
      age
    }
  }
`

export const UPDATE_PATIENT = gql`
  mutation UpdatePatient($command: UpdatePatientCommandInput!) {
    updatePatient(command: $command) {
      id
      firstName
      lastName
      fullName
      dateOfBirth
      age
    }
  }
`

export const DELETE_PATIENT = gql`
  mutation DeletePatient($command: DeletePatientInput!) {
    deletePatient(command: $command)
  }
`

export const ADD_DIAGNOSTIC_RESULT = gql`
  mutation AddDiagnosticResult($input: AddDiagnosticResultCommandInput!) {
    addDiagnosticResult(command: $input) {
      id
      patientId
      diagnosis
      notes
      timestampUtc
    }
  }
`

export const GET_DIAGNOSES = gql`
  query GetDiagnoses(
    $type: String
    $createdAfter: DateTime
    $createdBefore: DateTime
    $skip: Int
    $take: Int
  ) {
    diagnoses(
      type: $type
      createdAfter: $createdAfter
      createdBefore: $createdBefore
      skip: $skip
      take: $take
    ) {
      nodes {
        id
        patientId
        patient {
          id
          firstName
          lastName
          fullName
        }
        diagnosis
        notes
        timestampUtc
        createdAt
        isActive
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor
        endCursor
      }
    }
  }
`
