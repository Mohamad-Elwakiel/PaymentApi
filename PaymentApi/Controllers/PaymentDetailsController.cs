using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.DTOs;
using PaymentApi.Models;
    

namespace PaymentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentDetailsController : ControllerBase
    {
        private readonly PaymentDetailContext _context;
        private readonly IMapper _mapper;
        public PaymentDetailsController(PaymentDetailContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
           
        }

        // GET: api/PaymentDetails
        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<PaymentDetailDto>>> GetPaymentDetails()
        {
            var paymentDetails = await _context.PaymentDetails.ToListAsync();
            var paymentDetailsDto = _mapper.Map<List<PaymentDetailDto>>(paymentDetails); 
            return Ok(paymentDetailsDto);   
        }
       
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDetailDto>> GetPaymentDetail(int id)
        {
            var paymentDetail = await _context.PaymentDetails.FindAsync(id);
            var paymentDetailDto = _mapper.Map<PaymentDetailDto>(paymentDetail);    

            if (paymentDetail == null)
            {
                return NotFound();
            }

            return paymentDetailDto;
        }
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<PaymentDetailDto>>> GetPaymentDetail([FromQuery]int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
          
            var paymentDetails = await _context.PaymentDetails
                .Skip((pageIndex ) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var paymentDetailsDto = _mapper.Map<List<PaymentDetailDto>>(paymentDetails); 
            return Ok(paymentDetailsDto);   
        }

        // PUT: api/PaymentDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentDetail(int id, PaymentDetailDto  paymentDetailDto)
        {

            if (id != paymentDetailDto.PaymentDetailId)
            {
                return BadRequest();
            }
            var paymentDetail = _mapper.Map<PaymentDetail>(paymentDetailDto);   

            _context.Entry(paymentDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentDetailExists(id))
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

        // POST: api/PaymentDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PaymentDetailDto>> PostPaymentDetail(PaymentDetailDto paymentDetailDto)
        {
            var paymentDetail = _mapper.Map<PaymentDetail>(paymentDetailDto);
            _context.PaymentDetails.Add(paymentDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaymentDetail", new { id = paymentDetail.PaymentDetailId }, paymentDetail);
        }
        [HttpPost("PopulatData")]
      public async Task<ActionResult<PaymentDetail>> PopulatData()
        {
            for(int i = 1000; i < 10000; i++)
            {
                var paymentDetail = new PaymentDetail
                {
                    CardOwnerName = "User " + i,
                    CardNumber = "1234567890" + i,
                    ExpirationDate = "12/25",
                    SecurityCode = "123",
                    
                };
                _context.PaymentDetails.Add(paymentDetail);
                
            }
            await _context.SaveChangesAsync();
            return Ok();
              
        }
        // DELETE: api/PaymentDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentDetail(int id)
        {
            var paymentDetail = await _context.PaymentDetails.FindAsync(id);
            if (paymentDetail == null)
            {
                return NotFound();
            }

            _context.PaymentDetails.Remove(paymentDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentDetailExists(int id)
        {
            return _context.PaymentDetails.Any(e => e.PaymentDetailId == id);
        }
        [HttpGet("SearchByName")]
        public async Task<ActionResult<IEnumerable<PaymentDetailDto>>> Search(string name)
        {
            var paymentDetails = await _context.PaymentDetails
                .Where(x => x.CardOwnerName.Contains(name))
                .ToListAsync();
            var paymentDetailsDto = _mapper.Map<List<PaymentDetailDto>>(paymentDetails); 
            return Ok(paymentDetailsDto);   
        }
        [HttpGet("SearchByCardNumber")]
        public async Task<ActionResult<IEnumerable<PaymentDetailDto>>> SearchByCardNumber(string cardNumber)
        {
            var paymentDetails = await _context.PaymentDetails
                .Where(x => x.CardNumber.Contains(cardNumber))
                .ToListAsync();
            var paymentDetailsDto = _mapper.Map<List<PaymentDetailDto>>(paymentDetails); 
            return Ok(paymentDetailsDto);   
        }
        [HttpGet("SearchUser")]
        public async Task<ActionResult<IEnumerable<PaymentDetailDto>>> SearchUser([FromQuery] int pageIndex=1, [FromQuery] int pageSize =10, [FromQuery] string name = "", [FromQuery] int cardNumber = 0)
        {
            var parameters = new MyStoredProcedure
            {
                Name = name,
                CardNumber = cardNumber,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var paymentDetails = await _context.ExecuteMyStoredProcedureAsync(parameters);
                

            var paymentDetail = _mapper.Map<List<PaymentDetail>>(paymentDetails);
            return Ok(paymentDetail);
        }
    }
}
