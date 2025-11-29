# HealthHub - GraphQL Patient Management Implementation Plan

## Architektura s GraphQL API

### Backend Status (✅ Kompletní)
- **GraphQL API**: Plně funkční s HotChocolate
- **Query**: GetPatients, GetPatient, GetPatientDiagnosticResults
- **Mutation**: CreatePatient, UpdatePatient, AddDiagnosticResult, UpdateDiagnosticResult, DeletePatient
- **Autentizace**: JWT token s [Authorize] atributem
- **Paginace/Filtrování**: HotChocolate built-in support

### Frontend Status (❌ Potřeba implementace)
- **Integrace**: React frontend je integrován, ale chybí GraphQL klient
- **Komponenty**: Základní struktura existuje, ale chybí funkční komponenty
- **API**: Potřeba GraphQL service layer

## GraphQL Schema Overview

### Query Operations
```graphql
query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
  patients(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
    id
    firstName
    lastName
    fullName
    age
    lastDiagnosis
    createdAt
  }
}

query GetPatient($id: UUID!) {
  patient(id: $id) {
    id
    firstName
    lastName
    fullName
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

query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
  getPatientDiagnosticResults(patientId: $patientId, limit: $limit) {
    id
    diagnosis
    notes
    timestampUtc
  }
}
```

### Mutation Operations
```graphql
mutation CreatePatient($input: CreatePatientInput!) {
  createPatient(command: $input) {
    id
    firstName
    lastName
    fullName
    age
  }
}

mutation UpdatePatient($input: UpdatePatientInput!) {
  updatePatient(command: $input) {
    id
    firstName
    lastName
    fullName
    age
  }
}

mutation AddDiagnosticResult($input: AddDiagnosticResultInput!) {
  addDiagnosticResult(command: $input) {
    id
    patientId
    diagnosis
    notes
    timestampUtc
  }
}

mutation DeletePatient($input: DeletePatientInput!) {
  deletePatient(command: $input)
}
```

## Frontend Implementation Strategy

### 1. GraphQL Client Setup
**Nástroje**: Apollo Client nebo urql
**Konfigurace**:
- Base URL: http://localhost:5023/graphql (dev), /graphql (prod)
- Authentication: JWT token v headers
- Error handling: Global error boundaries
- Caching: Optimistic updates a cache management

### 2. Service Layer Architecture
```
services/
├── graphql-client.ts          # GraphQL client configuration
├── patient-service.ts         # Patient operations
├── diagnosis-service.ts       # Diagnosis operations
├── auth-service.ts            # Authentication
└── types.ts                   # TypeScript typy
```

### 3. React Components Structure
```
components/
├── patients/
│   ├── PatientList.tsx        # Seznam s GraphQL query
│   ├── PatientCard.tsx        # Karta pacienta
│   ├── PatientForm.tsx        # Formulář s GraphQL mutation
│   └── PatientSearch.tsx      # Vyhledávání
├── diagnoses/
│   ├── DiagnosisList.tsx      # Seznam diagnóz
│   ├── DiagnosisForm.tsx      # Formulář diagnózy
│   └── DiagnosisTimeline.tsx  # Timeline
└── shared/
    ├── GraphQLProvider.tsx    # GraphQL provider
    ├── LoadingSpinner.tsx
    └── ErrorBoundary.tsx
```

## Implementation Phases

### Fáze 1: GraphQL Infrastructure (1-2 dny)
1. **Nainstalovat GraphQL klient** (Apollo Client/urql)
2. **Nakonfigurovat GraphQL provider** s authentication
3. **Vytvořit základní service layer**
4. **Implementovat TypeScript typy** pro GraphQL schema

### Fáze 2: Patient Management (3-4 dny)
1. **PatientList komponenta** s paginací a vyhledáváním
2. **PatientForm komponenta** pro create/update operace
3. **PatientDetail stránka** s diagnostickými výsledky
4. **Delete pacienta** s confirmation dialog

### Fáze 3: Diagnosis Management (2-3 dny)
1. **DiagnosisList komponenta** pro historii diagnóz
2. **DiagnosisForm komponenta** pro přidání/úpravu
3. **DiagnosisTimeline** pro vizualizaci časové osy
4. **Real-time updates** pomocí GraphQL subscriptions (volitelné)

### Fáze 4: Advanced Features (2-3 dny)
1. **Dashboard** s pacient statistikami
2. **Search a filtering** s GraphQL variables
3. **Export funkcionalita**
4. **Responsive design** optimalizace

## GraphQL Client Implementation Details

### Apollo Client Configuration
```typescript
// services/graphql-client.ts
import { ApolloClient, InMemoryCache, createHttpLink } from '@apollo/client';
import { setContext } from '@apollo/client/link/context';

const httpLink = createHttpLink({
  uri: getGraphQLUrl(),
});

const authLink = setContext((_, { headers }) => {
  const token = localStorage.getItem('authToken');
  return {
    headers: {
      ...headers,
      authorization: token ? `Bearer ${token}` : "",
    }
  };
});

export const client = new ApolloClient({
  link: authLink.concat(httpLink),
  cache: new InMemoryCache(),
});
```

### Patient Service Example
```typescript
// services/patient-service.ts
import { gql } from '@apollo/client';

export const GET_PATIENTS = gql`
  query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
    patients(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
      id
      firstName
      lastName
      fullName
      age
      lastDiagnosis
      createdAt
    }
  }
`;

export const CREATE_PATIENT = gql`
  mutation CreatePatient($input: CreatePatientInput!) {
    createPatient(command: $input) {
      id
      firstName
      lastName
      fullName
      age
    }
  }
`;
```

## Benefits of GraphQL Approach

### Pro frontend vývoj
- **Přesné data fetching** - žádné over-fetching
- **Jediný endpoint** - jednodušší konfigurace
- **Type safety** - generované typy ze schema
- **Real-time updates** - subscriptions support

### Pro backend vývoj
- **Již implementováno** - žádná další práce
- **Flexibilní API** - klient si vybírá data
- **Built-in features** - paginace, filtrování, sorting

## Testing Strategy

### GraphQL Specific Testing
- **Mock GraphQL server** pro unit tests
- **Integration tests** s reálným GraphQL endpointem
- **Error handling tests** pro network errors
- **Cache behavior tests** pro Apollo Client

### End-to-End Testing
- **Cypress/E2E tests** pro kompletní workflow
- **GraphQL request/response validation**
- **Authentication flow testing**

## Deployment Considerations

### Production Build
- **Tree shaking** GraphQL queries
- **Code splitting** pro různé feature moduly
- **Cache configuration** pro optimalizaci

### Performance Optimization
- **Persisted queries** pro menší payload
- **Query batching** pro multiple requests
- **Cache strategies** pro offline support

## Success Metrics
- ✅ Všechny CRUD operace fungují přes GraphQL
- ✅ Responzivní design na všech zařízeních
- ✅ GraphQL caching a performance optimalizace
- ✅ User-friendly interface s error handling
- ✅ Performance benchmarks (page load < 2s)

## Estimated Timeline
**Fáze 1**: 1-2 dny  
**Fáze 2**: 3-4 dny  
**Fáze 3**: 2-3 dny  
**Fáze 4**: 2-3 dny  
**Testing**: 1-2 dny

**Celkem**: 9-14 dní