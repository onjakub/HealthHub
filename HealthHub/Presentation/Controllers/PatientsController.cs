using HealthHub.Application.DTOs;
using HealthHub.Application.Handlers;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthHub.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>> _getPatientsQueryHandler;
        private readonly IQueryHandler<GetPatientByIdQuery, PatientDetailDto?> _getPatientByIdQueryHandler;
        private readonly IPatientRepository _patientRepository;

        public PatientsController(
            IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>> getPatientsQueryHandler,
            IQueryHandler<GetPatientByIdQuery, PatientDetailDto?> getPatientByIdQueryHandler,
            IPatientRepository patientRepository)
        {
            _getPatientsQueryHandler = getPatientsQueryHandler;
            _getPatientByIdQueryHandler = getPatientByIdQueryHandler;
            _patientRepository = patientRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            try
            {
                var query = new GetPatientsQuery();
                var patients = await _getPatientsQueryHandler.Handle(query, CancellationToken.None);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailDto>> GetPatient(Guid id)
        {
            try
            {
                var query = new GetPatientByIdQuery { PatientId = id };
                var patient = await _getPatientByIdQueryHandler.Handle(query, CancellationToken.None);
                
                if (patient == null)
                {
                    return NotFound();
                }
                
                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetPatientsCount()
        {
            try
            {
                var count = await _patientRepository.GetCountAsync(CancellationToken.None);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}