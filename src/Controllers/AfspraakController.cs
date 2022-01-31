using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AfspraakController : ControllerBase
    {
        private readonly MijnContext _context;

        public AfspraakController(MijnContext context)
        {
            _context = context;
        }

        // GET: api/Afspraak
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Afspraak>>> GetAfspraken()
        {
            return await _context.Afspraken.ToListAsync();
        }

        // GET: api/Afspraak/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Afspraak>> GetAfspraak(int id)
        {
            var afspraak = await _context.Afspraken.FindAsync(id);

            if (afspraak == null)
            {
                return NotFound();
            }

            return afspraak;
        }

        // PUT: api/Afspraak/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAfspraak(int id, Afspraak afspraak)
        {
            if (id != afspraak.Id)
            {
                return BadRequest();
            }

            _context.Entry(afspraak).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AfspraakExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Afspraak
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Afspraak>> PostAfspraak(Afspraak afspraak)
        {
            foreach(var i in _context.Afspraken){

                //Deze regel zorgt ervoor dat een afspraak niet gemaakt kan worden als dezelfde orthopedagoog op hetzelfde tijdstip een afspraak heeft
                if(i.Pedagoog != afspraak.Pedagoog || !((i.datum < afspraak.datum&& i.Einddatum < afspraak.datum && i.Einddatum < afspraak.datum && i.Einddatum < afspraak.Einddatum)||(i.datum > afspraak.datum && i.datum > afspraak.Einddatum && i.Einddatum > afspraak.datum && i.Einddatum > afspraak.Einddatum))){
                    return NoContent();
                }
            }
            _context.Afspraken.Add(afspraak);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAfspraak", new { id = afspraak.Id }, afspraak);
        }

        // DELETE: api/Afspraak/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAfspraak(int id)
        {
            var afspraak = await _context.Afspraken.FindAsync(id);
            if (afspraak == null)
            {
                return NotFound();
            }

            _context.Afspraken.Remove(afspraak);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AfspraakExists(int id)
        {
            return _context.Afspraken.Any(e => e.Id == id);
        }
    }
}
