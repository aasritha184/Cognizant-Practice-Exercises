using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiHandsOn4.Models;

namespace WebApiHandsOn4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        /// Action method to return list of Employee objects.
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid employee id");
            }

            var emp = _employees.FirstOrDefault(e => e.Id == id);
            if (emp == null)
            {
                return BadRequest("Invalid employee id");
            }

            return Ok(emp);
        }

        /// <summary>
        /// Action method to create a new Employee object using [FromBody].
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> CreateEmployee([FromBody] Employee emp)
        {
            if (emp == null)
            {
                return BadRequest("Invalid employee data");
            }

            if (emp.Id <= 0)
            {
                emp.Id = _employees.Any() ? _employees.Max(e => e.Id) + 1 : 1;
            }

            _employees.Add(emp);
            return CreatedAtAction(nameof(GetById), new { id = emp.Id }, emp);
        }

        /// <summary>
        /// Action method to update Employee data using [FromBody] payload.
        /// Checks if id <= 0 or not found in hardcoded list, returning BadRequest("Invalid employee id").
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> UpdateEmployee([FromBody] Employee emp)
        {
            if (emp == null || emp.Id <= 0)
            {
                return BadRequest("Invalid employee id");
            }

            var existingEmployee = _employees.FirstOrDefault(e => e.Id == emp.Id);
            if (existingEmployee == null)
            {
                return BadRequest("Invalid employee id");
            }

            // Update the employee record in the hardcoded list
            existingEmployee.Name = emp.Name;
            existingEmployee.Salary = emp.Salary;
            existingEmployee.Permanent = emp.Permanent;
            existingEmployee.Department = emp.Department;
            existingEmployee.Skills = emp.Skills;
            existingEmployee.DateOfBirth = emp.DateOfBirth;

            // Filter the employee list data for the input id and return that as the output
            var updatedEmployee = _employees.First(e => e.Id == emp.Id);
            return Ok(updatedEmployee);
        }

        /// <summary>
        /// Action method to update Employee data with ID passed in route parameter.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Employee> UpdateEmployeeWithId(int id, [FromBody] Employee emp)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid employee id");
            }

            var existingEmployee = _employees.FirstOrDefault(e => e.Id == id);
            if (existingEmployee == null)
            {
                return BadRequest("Invalid employee id");
            }

            if (emp != null)
            {
                existingEmployee.Name = emp.Name;
                existingEmployee.Salary = emp.Salary;
                existingEmployee.Permanent = emp.Permanent;
                existingEmployee.Department = emp.Department;
                existingEmployee.Skills = emp.Skills;
                existingEmployee.DateOfBirth = emp.DateOfBirth;
            }

            var updatedEmployee = _employees.First(e => e.Id == id);
            return Ok(updatedEmployee);
        }

        /// <summary>
        /// Action method to delete an Employee from the hardcoded list by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult DeleteEmployee(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid employee id");
            }

            var emp = _employees.FirstOrDefault(e => e.Id == id);
            if (emp == null)
            {
                return BadRequest("Invalid employee id");
            }

            _employees.Remove(emp);
            return Ok(new { message = $"Employee with Id = {id} successfully deleted." });
        }
    }
}
