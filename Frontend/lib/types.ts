// GraphQL Response Types
export interface GraphQLResponse<T> {
  data?: T;
  errors?: GraphQLError[];
}

export interface GraphQLError {
  message: string;
  path?: string[];
  locations?: Array<{ line: number; column: number }>;
}

// Patient Types
export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  lastDiagnosis?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PatientDetail extends Patient {
  diagnosticResults: DiagnosticResult[];
}

export interface DiagnosticResult {
  id: string;
  patientId: string;
  diagnosis: string;
  notes?: string;
  timestampUtc: string;
  createdAt: string;
}

// GraphQL Query Response Types
export interface GetPatientsResponse {
  patients: {
    nodes: Patient[];
    pageInfo: {
      hasNextPage: boolean;
      hasPreviousPage: boolean;
      startCursor?: string;
      endCursor?: string;
    };
    totalCount: number;
  };
}

export interface GetPatientResponse {
  patient: PatientDetail | null;
}

export interface GetPatientDiagnosticResultsResponse {
  patientDiagnosticResults: DiagnosticResult[];
}

export interface GetDiagnosesResponse {
  diagnoses: {
    nodes: DiagnosticResult[];
    pageInfo: {
      hasNextPage: boolean;
      hasPreviousPage: boolean;
      startCursor?: string;
      endCursor?: string;
    };
    totalCount: number;
  };
}

// GraphQL Mutation Input Types
export interface CreatePatientInput {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
}

export interface UpdatePatientInput {
  patientId: string;
  firstName?: string;
  lastName?: string;
  dateOfBirth?: string;
}

export interface AddDiagnosticResultInput {
  patientId: string;
  diagnosis: string;
  notes?: string;
}

export interface UpdateDiagnosticResultInput {
  diagnosticResultId: string;
  diagnosis?: string;
  notes?: string;
}

export interface DeletePatientInput {
  patientId: string;
}

// GraphQL Mutation Response Types
export interface CreatePatientResponse {
  createPatient: Patient;
}

export interface UpdatePatientResponse {
  updatePatient: Patient;
}

export interface AddDiagnosticResultResponse {
  addDiagnosticResult: DiagnosticResult;
}

export interface UpdateDiagnosticResultResponse {
  updateDiagnosticResult: DiagnosticResult;
}

export interface DeletePatientResponse {
  deletePatient: boolean;
}

// Component Props Types
export interface PatientFormProps {
  patient?: Patient | null;
  onSuccess: () => void;
  onCancel: () => void;
  onResetToAdd?: () => void;
}

export interface PatientListProps {
  onEditPatient: (patient: Patient) => void;
}

export interface LoginFormProps {
  onLoginSuccess: (token: string) => void;
}

// API Response Types
export interface AuthResponse {
  token: string;
  expiresIn?: number;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: unknown;
}

// Utility Types
export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface PaginationState {
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PaginationInfo {
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startCursor?: string;
  endCursor?: string;
}

export interface SortParams {
  field: string;
  direction: 'ASC' | 'DESC';
}