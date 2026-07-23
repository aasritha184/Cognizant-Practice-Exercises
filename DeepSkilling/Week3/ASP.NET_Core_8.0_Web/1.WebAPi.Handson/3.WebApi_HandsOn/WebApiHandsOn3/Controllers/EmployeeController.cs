using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiHandsOn3.Filters;
using WebApiHandsOn3.Models;

namespace WebApiHandsOn3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthFilter]
    [TypeFilter(typeof(CustomExceptionFilter))]
    public class EmployeeController : ControllerBase
    {
        private static List<Employee> _employees = new List<Employee>();

        public EmployeeController()
        {
            if (!_employees.Any())
            {
                _employees = GetStandardEmployeeList();
            }
        }

        /// <summary>
        /// Private method that initializes and returns a standard list of Employee objects.
        /// </summary>
        private List<Employee> GetStandardEmployeeList()
        {
            return new List<Employee>
            {
                new Employee
                {
                    Id = 1,
                    Name = "John Doe",
                    Salary = 75000,
                    Permanent = true,
                    Department = new Department { Id = 101, Name = "IT Infrastructure" },
                    Skills = new List<Skill>
                    {
                        new Skill { Id = 1, Name = "C#" },
                        new Skill { Id = 2, Name = "ASP.NET Core" },
                        new Skill { Id = 3, Name = "SQL Server" }
                    },
                    DateOfBirth = new DateTime(1995, 5, 15)
                },
                new Employee
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Salary = 85000,
                    Permanent = true,
                    Department = new Department { Id = 102, Name = "Software Development" },
                    Skills = new List<Skill>
                    {
                        new Skill { Id = 4, Name = "Angular" },
                        new Skill { Id = 5, Name = "TypeScript" },
                        new Skill { Id = 6, Name = "REST Web API" }
                    },
                    DateOfBirth = new DateTime(1992, 8, 22)
                },
                new Employee
                {
                    Id = 3,
                    Name = "Robert Johnson",
                    Salary = 62000,
                    Permanent = false,
                    Department = new Department { Id = 103, Name = "Human Resources" },
                    Skills = new List<Skill>
                    {
                        new Skill { Id = 7, Name = "Recruitment" },
                        new Skill { Id = 8, Name = "Payroll" }
                    },
                    DateOfBirth = new DateTime(1998, 11, 3)
                }
            };
        }

        /// <summary>
        /// Action method to return list of custom class entity standard list with [AllowAnonymous].
        /// </summary>
        [HttpGet("standard")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        public ActionResult<List<Employee>> GetStandard()
        {
            return Ok(GetStandardEmployeeList());
        }

        /// <summary>
        /// Action method to return list of Employee objects (Requires Bearer authorization filter).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        public ActionResult<List<Employee>> Get()
        {
            return Ok(_employees);
        }

        /// <summary>
        /// Action method to return single Employee by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Employee> GetById(int id)
        {
            var emp = _employees.FirstOrDefault(e => e.Id == id);
            if (emp == null)
            {
                return NotFound($"Employee with Id = {id} not found.");
            }
            return Ok(emp);
        }

        /// <summary>
        /// Action method that deliberately throws an exception to demonstrate CustomExceptionFilter.
        /// </summary>
        [HttpGet("exception")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Employee>> GetWithException()
        {
            throw new InvalidOperationException("Simulated exception in GET action method to test CustomExceptionFilter file logging!");
        }

        /// <summary>
        /// Demonstrates usage of [FromBody] attribute to read Employee model object from request payload.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> CreateEmployee([FromBody] Employee emp)
        {
            if (emp == null)
            {
                return BadRequest("Invalid employee data provided.");
            }

            if (emp.Id <= 0)
            {
                emp.Id = _employees.Max(e => e.Id) + 1;
            }

            _employees.Add(emp);
            return CreatedAtAction(nameof(GetById), new { id = emp.Id }, emp);
        }

        /// <summary>
        /// Demonstrates usage of [FromBody] attribute for PUT update operations.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Employee> UpdateEmployee(int id, [FromBody] Employee emp)
        {
            if (emp == null || id != emp.Id)
            {
                return BadRequest("Employee ID mismatch or null data.");
            }

            var existing = _employees.FirstOrDefault(e => e.Id == id);
            if (existing == null)
            {
                return NotFound($"Employee with Id = {id} not found.");
            }

            existing.Name = emp.Name;
            existing.Salary = emp.Salary;
            existing.Permanent = emp.Permanent;
            existing.Department = emp.Department;
            existing.Skills = emp.Skills;
            existing.DateOfBirth = emp.DateOfBirth;

            return Ok(existing);
        }
    }
}
