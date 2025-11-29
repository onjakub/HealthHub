# HealthHub Patient Management - Implementation Plan

## Current State Analysis

### Backend Status
✅ **Implemented:**
- CQRS architecture with commands and queries
- Patient and DiagnosticResult domain entities
- Repository interfaces and implementations
- GET endpoints for patients (list, by ID, count)
- Command handlers for CreatePatient, UpdatePatient, AddDiagnosticResult, UpdateDiagnosticResult, DeletePatient
- Query handlers for GetPatients, GetPatientById, GetPatientDiagnosticResults

❌ **Missing:**
- POST, PUT, DELETE endpoints in PatientsController
- API endpoints for diagnosis management
- Proper DTO validation
- Error handling middleware

### Frontend Status
✅ **Implemented:**
- React frontend built and integrated with ASP.NET Core
- Static file serving configured
- SPA routing working
- Basic page structure exists

❌ **Missing:**
- Patient management components
- Diagnosis management components
- API integration layer
- Form validation
- Search and filtering features

## Phase 1: Complete Backend API (Priority 1)

### 1.1 Update PatientsController
Add missing CRUD endpoints to [`PatientsController.cs`](HealthHub/Presentation/Controllers/PatientsController.cs:1):

```csharp
[HttpPost]
public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] CreatePatientDto createPatientDto)

[HttpPut("{id}")]
public async Task<ActionResult<PatientDto>> UpdatePatient(Guid id, [FromBody] UpdatePatientDto updatePatientDto)

[HttpDelete("{id}")]
public async Task<ActionResult> DeletePatient(Guid id)

[HttpGet("{id}/diagnoses")]
public async Task<ActionResult<IEnumerable<DiagnosticResultDto>>> GetPatientDiagnoses(Guid id)

[HttpPost("{id}/diagnoses")]
public async Task<ActionResult<DiagnosticResultDto>> AddDiagnosis(Guid id, [FromBody] CreateDiagnosticResultDto createDiagnosticResultDto)

[HttpPut("diagnoses/{diagnosisId}")]
public async Task<ActionResult<DiagnosticResultDto>> UpdateDiagnosis(Guid diagnosisId, [FromBody] UpdateDiagnosticResultDto updateDiagnosticResultDto)

[HttpDelete("diagnoses/{diagnosisId}")]
public async Task<ActionResult> DeleteDiagnosis(Guid diagnosisId)
```

### 1.2 Add Missing DTOs
Create missing DTO classes in [`Application/DTOs/`](HealthHub/Application/DTOs/):
- `CreateDiagnosticResultDto`
- `UpdateDiagnosticResultDto`

### 1.3 Add Validation
Implement data validation using FluentValidation or DataAnnotations.

## Phase 2: React Frontend Implementation (Priority 2)

### 2.1 Create API Service Layer
Create `services/` directory with:
- `patientService.ts` - API calls for patient operations
- `diagnosisService.ts` - API calls for diagnosis operations
- `apiClient.ts` - HTTP client configuration

### 2.2 Implement Patient Management Components
Create `components/patients/` directory:
- `PatientList.tsx` - Patient listing with search/filter
- `PatientCard.tsx` - Individual patient card component
- `PatientForm.tsx` - Create/Edit patient form
- `PatientSearch.tsx` - Search and filter component

### 2.3 Implement Diagnosis Management Components
Create `components/diagnoses/` directory:
- `DiagnosisList.tsx` - Diagnosis history for a patient
- `DiagnosisForm.tsx` - Add/Edit diagnosis form
- `DiagnosisTimeline.tsx` - Timeline view of diagnoses

### 2.4 Create Pages
Update existing pages to use real data:
- `app/page.tsx` - Patient dashboard
- `app/patient/[id]/page.tsx` - Patient details with diagnoses

## Phase 3: Advanced Features (Priority 3)

### 3.1 Search and Filtering
- Implement search by patient name, diagnosis
- Add filters by date range, diagnosis type
- Pagination support

### 3.2 Dashboard and Reporting
- Patient statistics dashboard
- Diagnosis trends and reports
- Data export functionality

### 3.3 User Experience
- Loading states and error handling
- Success/error notifications
- Form validation feedback
- Responsive design improvements

## Implementation Details

### Backend API Endpoints Specification

#### Patients Endpoints
```
GET    /api/patients                    - Get all patients (with search/pagination)
GET    /api/patients/{id}              - Get patient by ID
POST   /api/patients                    - Create new patient
PUT    /api/patients/{id}              - Update patient
DELETE /api/patients/{id}              - Delete patient
GET    /api/patients/count             - Get patient count
GET    /api/patients/{id}/diagnoses    - Get patient diagnoses
POST   /api/patients/{id}/diagnoses    - Add diagnosis to patient
PUT    /api/patients/diagnoses/{id}    - Update diagnosis
DELETE /api/patients/diagnoses/{id}    - Delete diagnosis
```

### Frontend Component Structure

```
components/
├── patients/
│   ├── PatientList.tsx
│   ├── PatientCard.tsx
│   ├── PatientForm.tsx
│   ├── PatientSearch.tsx
│   └── PatientFilters.tsx
├── diagnoses/
│   ├── DiagnosisList.tsx
│   ├── DiagnosisCard.tsx
│   ├── DiagnosisForm.tsx
│   └── DiagnosisTimeline.tsx
├── shared/
│   ├── LoadingSpinner.tsx
│   ├── ErrorMessage.tsx
│   ├── SuccessMessage.tsx
│   └── ConfirmationDialog.tsx
└── layout/
    ├── Header.tsx
    ├── Sidebar.tsx
    └── Footer.tsx
```

### Data Models (TypeScript)

```typescript
interface Patient {
  id: string
  firstName: string
  lastName: string
  fullName: string
  dateOfBirth: string
  age: number
  lastDiagnosis?: string
  createdAt: string
  updatedAt?: string
}

interface PatientDetail extends Patient {
  diagnosticResults: Diagnosis[]
}

interface Diagnosis {
  id: string
  patientId: string
  diagnosis: string
  notes?: string
  timestampUtc: string
  createdAt: string
}
```

## Testing Strategy

### Backend Tests
- Unit tests for command/query handlers
- Integration tests for API endpoints
- Repository tests with in-memory database

### Frontend Tests
- Unit tests for components and services
- Integration tests for user workflows
- E2E tests for critical paths

## Deployment Considerations

### Build Process
- Separate build scripts for frontend and backend
- Docker containerization
- Environment-specific configurations

### Performance Optimization
- Frontend code splitting
- API response caching
- Database query optimization

## Success Criteria

- All CRUD operations functional for patients and diagnoses
- Responsive design working on all devices
- API integration with proper error handling
- User-friendly interface with accessibility compliance
- Performance benchmarks met (page load < 2s, API response < 200ms)

## Timeline Estimate

**Phase 1 (Backend API):** 2-3 days
**Phase 2 (Frontend Implementation):** 3-4 days  
**Phase 3 (Advanced Features):** 2-3 days
**Testing and Polish:** 1-2 days

**Total Estimated Time:** 8-12 days