using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace WebAPIHandson.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // Static in-memory collection to simulate persistent storage during application lifecycle
        private static readonly List<string> _values = new List<string> { "value1", "value2", "value3" };

        // GET: api/values
        /// <summary>
        /// Retrieves all values (HTTP GET - 200 OK)
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(_values);
        }

        // GET api/values/5
        /// <summary>
        /// Retrieves a value by ID (HTTP GET - 200 OK / 400 BadRequest / 404 NotFound)
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            if (id < 0)
            {
                return BadRequest("ID must be a positive integer.");
            }

            if (id >= _values.Count)
            {
                return NotFound($"Value with ID {id} not found.");
            }

            return Ok(_values[id]);
        }

        // POST api/values
        /// <summary>
        /// Creates a new value (HTTP POST - 201 Created / 400 BadRequest)
        /// </summary>
        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return BadRequest("Value cannot be null or empty.");
            }

            _values.Add(value);
            int newId = _values.Count - 1;

            return CreatedAtAction(nameof(Get), new { id = newId }, new { id = newId, value });
        }

        // PUT api/values/5
        /// <summary>
        /// Updates an existing value by ID (HTTP PUT - 200 OK / 400 BadRequest / 404 NotFound)
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] string value)
        {
            if (id < 0 || id >= _values.Count)
            {
                return NotFound($"Value with ID {id} not found.");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return BadRequest("Updated value cannot be null or empty.");
            }

            _values[id] = value;
            return Ok(new { message = $"Value at ID {id} updated successfully.", updatedValue = value });
        }

        // DELETE api/values/5
        /// <summary>
        /// Deletes a value by ID (HTTP DELETE - 200 OK / 404 NotFound)
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (id < 0 || id >= _values.Count)
            {
                return NotFound($"Value with ID {id} not found.");
            }

            string removedValue = _values[id];
            _values.RemoveAt(id);

            return Ok(new { message = $"Value at ID {id} deleted successfully.", deletedValue = removedValue });
        }

        // GET api/values/unauthorized-demo
        /// <summary>
        /// Demonstrates HTTP 401 Unauthorized status code
        /// </summary>
        [HttpGet("unauthorized-demo")]
        public ActionResult GetUnauthorizedDemo()
        {
            return Unauthorized("Access denied: You do not have permission to access this resource.");
        }

        // GET api/values/error-demo
        /// <summary>
        /// Demonstrates HTTP 500 InternalServerError status code
        /// </summary>
        [HttpGet("error-demo")]
        public ActionResult GetErrorDemo()
        {
            return StatusCode(500, "An internal server error occurred while processing your request.");
        }
    }
}
