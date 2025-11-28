class HealthHubApp {
    constructor() {
        this.token = localStorage.getItem('healthhub_token');
        this.currentUser = localStorage.getItem('healthhub_user');
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.checkAuthStatus();
    }

    setupEventListeners() {
        document.getElementById('add-patient-form').addEventListener('submit', (e) => {
            e.preventDefault();
            this.addPatient();
        });
    }

    checkAuthStatus() {
        if (this.token && this.currentUser) {
            this.showAuthenticatedUI();
        } else {
            this.showLoginUI();
        }
    }

    showLoginUI() {
        document.getElementById('login-form').style.display = 'block';
        document.getElementById('user-info').style.display = 'none';
        document.getElementById('login-message').style.display = 'block';
        document.getElementById('app-content').style.display = 'none';
    }

    showAuthenticatedUI() {
        document.getElementById('login-form').style.display = 'none';
        document.getElementById('user-info').style.display = 'block';
        document.getElementById('user-name').textContent = this.currentUser;
        document.getElementById('login-message').style.display = 'none';
        document.getElementById('app-content').style.display = 'block';
        
        this.loadPatients();
    }

    async login() {
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        if (!username || !password) {
            this.showError('Prosím vyplňte uživatelské jméno a heslo');
            return;
        }

        this.showLoading(true);

        try {
            const response = await fetch('/auth/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password })
            });

            if (response.ok) {
                const data = await response.json();
                this.token = data.token;
                this.currentUser = username;
                
                localStorage.setItem('healthhub_token', this.token);
                localStorage.setItem('healthhub_user', username);
                
                this.showAuthenticatedUI();
            } else {
                this.showError('Přihlášení selhalo. Zkontrolujte přihlašovací údaje.');
            }
        } catch (error) {
            this.showError('Chyba při přihlášení: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    logout() {
        this.token = null;
        this.currentUser = null;
        localStorage.removeItem('healthhub_token');
        localStorage.removeItem('healthhub_user');
        this.showLoginUI();
    }

    async loadPatients() {
        this.showLoading(true);

        try {
            const query = `
                query {
                    patients {
                        nodes {
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

            const response = await this.graphqlRequest(query);
            const patients = response.data.patients.nodes;
            this.displayPatients(patients);
        } catch (error) {
            this.showError('Chyba při načítání pacientů: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    async searchPatients() {
        const searchTerm = document.getElementById('search-input').value;
        
        if (searchTerm.length < 2) {
            if (searchTerm.length === 0) {
                this.loadPatients();
            }
            return;
        }

        this.showLoading(true);

        try {
            const query = `
                query {
                    patients {
                        nodes {
                            id
                            firstName
                            lastName
                            fullName
                            age
                            lastDiagnosis
                            createdAt
                        }
                    }
                }
            `;

            const response = await this.graphqlRequest(query);
            const allPatients = response.data.patients.nodes;
            
            // Filter patients client-side
            const filteredPatients = allPatients.filter(patient =>
                patient.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                patient.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                patient.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (patient.lastDiagnosis && patient.lastDiagnosis.toLowerCase().includes(searchTerm.toLowerCase()))
            );
            
            this.displayPatients(filteredPatients);
        } catch (error) {
            this.showError('Chyba při hledání pacientů: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    async loadPatientDetail(patientId) {
        this.showLoading(true);

        try {
            const query = `
                query($id: UUID!) {
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
            `;

            const response = await this.graphqlRequest(query, { id: patientId });
            const patient = response.data.patient;
            this.displayPatientDetail(patient);
        } catch (error) {
            this.showError('Chyba při načítání detailu pacienta: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    async addPatient() {
        const firstName = document.getElementById('new-first-name').value;
        const lastName = document.getElementById('new-last-name').value;
        const dob = document.getElementById('new-dob').value;

        if (!firstName || !lastName || !dob) {
            this.showError('Prosím vyplňte všechna pole');
            return;
        }

        this.showLoading(true);

        try {
            const mutation = `
                mutation($input: CreatePatientInput!) {
                    createPatient(command: $input) {
                        id
                        firstName
                        lastName
                        fullName
                        age
                    }
                }
            `;

            const variables = {
                input: {
                    firstName,
                    lastName,
                    dateOfBirth: dob
                }
            };

            await this.graphqlRequest(mutation, variables);
            
            // Reset form and show patients list
            document.getElementById('add-patient-form').reset();
            showTab('patients');
            this.loadPatients();
            
            this.showError('Pacient byl úspěšně přidán', 'success');
        } catch (error) {
            this.showError('Chyba při přidávání pacienta: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    async graphqlRequest(query, variables = {}) {
        const response = await fetch('/graphql', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': this.token ? `Bearer ${this.token}` : ''
            },
            body: JSON.stringify({
                query,
                variables
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        
        if (result.errors) {
            throw new Error(result.errors[0].message);
        }

        return result;
    }

    displayPatients(patients) {
        const container = document.getElementById('patients-list');
        
        if (patients.length === 0) {
            container.innerHTML = '<p class="message">Žádní pacienti nebyli nalezeni</p>';
            return;
        }

        container.innerHTML = patients.map(patient => `
            <div class="patient-card" onclick="app.loadPatientDetail('${patient.id}')">
                <h3>${patient.fullName}</h3>
                <p><strong>Věk:</strong> ${patient.age} let</p>
                <p><strong>Poslední diagnóza:</strong> ${patient.lastDiagnosis || 'Žádná'}</p>
                <p><strong>Přidáno:</strong> ${new Date(patient.createdAt).toLocaleDateString('cs-CZ')}</p>
            </div>
        `).join('');
    }

    displayPatientDetail(patient) {
        const container = document.getElementById('patient-detail');
        
        container.innerHTML = `
            <h2>Detail pacienta: ${patient.fullName}</h2>
            <div class="patient-info">
                <p><strong>Jméno:</strong> ${patient.firstName}</p>
                <p><strong>Příjmení:</strong> ${patient.lastName}</p>
                <p><strong>Věk:</strong> ${patient.age} let</p>
                <p><strong>Datum přidání:</strong> ${new Date(patient.createdAt).toLocaleDateString('cs-CZ')}</p>
            </div>
            
            <div class="diagnostic-results">
                <h3>Diagnostické výsledky</h3>
                ${patient.diagnosticResults.length > 0 ? 
                    patient.diagnosticResults.map(result => `
                        <div class="diagnostic-result">
                            <h4>${result.diagnosis}</h4>
                            <p><strong>Datum:</strong> ${new Date(result.timestampUtc).toLocaleDateString('cs-CZ')}</p>
                            ${result.notes ? `<p><strong>Poznámky:</strong> ${result.notes}</p>` : ''}
                        </div>
                    `).join('') : 
                    '<p>Žádné diagnostické výsledky</p>'
                }
            </div>
            
            <button onclick="app.hidePatientDetail()" style="margin-top: 1rem;">Zpět na seznam</button>
        `;
        
        container.style.display = 'block';
        document.getElementById('patients-list').style.display = 'none';
    }

    hidePatientDetail() {
        document.getElementById('patient-detail').style.display = 'none';
        document.getElementById('patients-list').style.display = 'block';
    }

    showLoading(show) {
        document.getElementById('loading').style.display = show ? 'block' : 'none';
    }

    showError(message, type = 'error') {
        const errorDiv = document.getElementById('error-message');
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
        errorDiv.className = type === 'success' ? 'error-message success' : 'error-message';
        
        if (type === 'success') {
            errorDiv.style.background = '#27ae60';
        }
        
        setTimeout(() => {
            errorDiv.style.display = 'none';
        }, 5000);
    }
}

// Global functions for HTML event handlers
function login() {
    app.login();
}

function logout() {
    app.logout();
}

function showTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.tab-button').forEach(button => {
        button.classList.remove('active');
    });
    
    // Show selected tab
    document.getElementById(`${tabName}-tab`).classList.add('active');
    document.querySelector(`.tab-button[onclick="showTab('${tabName}')"]`).classList.add('active');
    
    if (tabName === 'patients') {
        app.loadPatients();
    }
}

function searchPatients() {
    app.searchPatients();
}

// Initialize app
const app = new HealthHubApp();