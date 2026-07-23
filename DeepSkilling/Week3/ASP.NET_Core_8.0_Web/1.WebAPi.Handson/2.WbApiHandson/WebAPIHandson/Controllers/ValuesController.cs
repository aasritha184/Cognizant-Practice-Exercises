using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPIHandson.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static readonly List<string> _values = new List<string> { "value1", "value2", "value3" };

        // GET: api/values
        [HttpGet(Name = "GetAllValues")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(_values);
        }

        // GET api/values/5
        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Get(int id)
        {
            if (id < 0 || id >= _values.Count) return NotFound();
            return Ok(_values[id]);
        }

        // POST api/values
        [HttpPost(Name = "CreateValue")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return BadRequest();
            _values.Add(value);
            return CreatedAtRoute("GetValueById", new { id = _values.Count - 1 }, value);
        }

        // PUT api/values/5
        [HttpPut("{id}", Name = "UpdateValue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Put(int id, [FromBody] string value)
        {
            if (id < 0 || id >= _values.Count) return NotFound();
            _values[id] = value;
            return Ok(_values[id]);
        }

        // DELETE api/values/5
        [HttpDelete("{id}", Name = "DeleteValue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            if (id < 0 || id >= _values.Count) return NotFound();
            _values.RemoveAt(id);
            return Ok();
        }
    }
}
