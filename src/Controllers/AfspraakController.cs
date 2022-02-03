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
            return await _context.Afspraken.OrderBy(x => x.datum).ToListAsync();
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
        public async Task<ActionResult<Afspraak>> PostAfspraak([Bind("datum, EindDatum, ClientId, PedagoogId, Beschrijving")]Afspraak afspraak)
        {
            System.Console.WriteLine("Dit is een test");
            bool iets = true;
            foreach(var i in _context.Afspraken){   
                if(i.PedagoogId == afspraak.PedagoogId){
                    Console.WriteLine(i.datum);
                    Console.WriteLine(i.Einddatum);
                    Console.WriteLine(afspraak.datum);
                    Console.WriteLine(afspraak.Einddatum);

                //Deze regel zorgt ervoor dat een afspraak niet gemaakt kan worden als dezelfde orthopedagoog op hetzelfde tijdstip een afspraak heeft
                if((DateTime.Compare(i.datum, afspraak.datum) >= 0 && DateTime.Compare(i.datum, afspraak.Einddatum) <= 0) ||(DateTime.Compare(i.Einddatum, i.datum) >= 0 && DateTime.Compare(i.Einddatum, i.datum) <= 0)|| (DateTime.Compare(i.datum, afspraak.datum) <= 0 && DateTime.Compare(i.Einddatum, afspraak.Einddatum) >= 0) || (DateTime.Compare(i.datum, afspraak.datum) >= 0 && DateTime.Compare(i.Einddatum, i.Einddatum) <= 0)){
                    iets = false;
            }}}
            if(iets == true){
            _context.Afspraken.Add(afspraak);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAfspraak", new { id = afspraak.Id }, afspraak);}
            else{
                return NoContent();
            }
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
