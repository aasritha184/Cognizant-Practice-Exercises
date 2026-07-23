using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiHandsOn5.Models;

namespace WebApiHandsOn5.Controllers
{
    [Authorize]
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
                }
            };
        }

        /// <summary>
        /// Action method to return list of Employee objects (Requires valid JWT Bearer token and 'Admin' or 'POC' role).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,POC")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<List<Employee>> Get()
        {
            return Ok(_employees);
        }

        /// <summary>
        /// Action method requiring 'POC' role exclusively. Used to verify 403 Forbidden / 401 when requested with an 'Admin' token.
        /// </summary>
        [HttpGet("poc-only")]
        [Authorize(Roles = "POC")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<Employee>> GetPocOnly()
        {
            return Ok(_employees);
        }

        /// <summary>
        /// Action method requiring 'Admin' or 'POC' role. Used to verify 200 OK when requested with an 'Admin' token.
        /// </summary>
        [HttpGet("admin-poc")]
        [Authorize(Roles = "Admin,POC")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<List<Employee>> GetAdminPoc()
        {
            return Ok(_employees);
        }
    }
}
