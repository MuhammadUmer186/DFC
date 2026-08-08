using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
namespace RestaurantSystem.Controllers
{
    
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(CatDto dto)
        {
            await _service.CreateAsync(dto);
            return Ok("Category created");
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CatDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok("Category updated");
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Category deleted");
        }
    }
}

