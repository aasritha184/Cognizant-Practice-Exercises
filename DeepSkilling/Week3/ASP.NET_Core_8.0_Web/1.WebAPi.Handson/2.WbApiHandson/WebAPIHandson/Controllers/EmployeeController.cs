using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WebAPIHandson.Models;

namespace WebAPIHandson.Controllers
{
    // Step 3 Requirement: Modified Route attribute to 'Emp' (api/Emp)
    [Route("api/Emp")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private static readonly List<Employee> _employees = new List<Employee>
        {
            new Employee { Id = 101, Name = "Alice Smith", Department = "IT", Salary = 75000 },
            new Employee { Id = 102, Name = "Bob Johnson", Department = "HR", Salary = 62000 },
            new Employee { Id = 103, Name = "Charlie Brown", Department = "Finance", Salary = 80000 }
        };

        // GET: api/Emp
        /// <summary>
        /// Retrieves all employees list (GET action method)
        /// Demonstration of HttpGet with Name attribute and ProducesResponseType
        /// </summary>
        [HttpGet(Name = "GetAllEmployees")]
        [ActionName("GetEmpList")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Employee>))]
        public ActionResult<IEnumerable<Employee>> Get()
        {
            return Ok(_employees);
        }

        // GET: api/Emp/101
        /// <summary>
        /// Retrieves employee by ID
        /// </summary>
        [HttpGet("{id}", Name = "GetEmployeeById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            var emp = _employees.FirstOrDefault(e => e.Id == id);
            if (emp == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }

            return Ok(emp);
        }

        // POST: api/Emp
        /// <summary>
        /// Adds a new employee
        /// </summary>
        [HttpPost(Name = "CreateEmployee")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] Employee newEmp)
        {
            if (newEmp == null || string.IsNullOrWhiteSpace(newEmp.Name))
            {
                return BadRequest("Invalid employee data.");
            }

            if (newEmp.Id <= 0)
            {
                newEmp.Id = _employees.Max(e => e.Id) + 1;
            }

            _employees.Add(newEmp);
            return CreatedAtRoute("GetEmployeeById", new { id = newEmp.Id }, newEmp);
        }

        // PUT: api/Emp/101
        /// <summary>
        /// Updates existing employee
        /// </summary>
        [HttpPut("{id}", Name = "UpdateEmployee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Put(int id, [FromBody] Employee updatedEmp)
        {
            if (updatedEmp == null) return BadRequest("Updated data cannot be null.");

            var existing = _employees.FirstOrDefault(e => e.Id == id);
            if (existing == null) return NotFound($"Employee with ID {id} not found.");

            existing.Name = updatedEmp.Name;
            existing.Department = updatedEmp.Department;
            existing.Salary = updatedEmp.Salary;

            return Ok(new { message = $"Employee ID {id} updated successfully.", employee = existing });
        }

        // DELETE: api/Emp/101
        /// <summary>
        /// Deletes employee by ID
        /// </summary>
        [HttpDelete("{id}", Name = "DeleteEmployee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            var existing = _employees.FirstOrDefault(e => e.Id == id);
            if (existing == null) return NotFound($"Employee with ID {id} not found.");

            _employees.Remove(existing);
            return Ok(new { message = $"Employee ID {id} deleted successfully." });
        }
    }
}
