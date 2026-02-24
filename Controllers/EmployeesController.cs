using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using migration_accelerator.Models;

public class EmployeesController : Controller
{
    private readonly AppDbContext _context;
/// <summary>
/// Constructor to inject the database context into the controller, allowing it to perform CRUD operations on the Employees table.
/// </summary>
/// <param name="context"></param>
    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    // READ
    public async Task<IActionResult> Index()
    {
        return View(await _context.Employees.ToListAsync());
    }

    // CREATE - GET
    public IActionResult Create()
    {
        return View();
    }

    // CREATE - POST
    [HttpPost]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // EDIT - GET
    public async Task<IActionResult> Edit(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        return View(emp);
    }

    // EDIT - POST
    [HttpPost]
    public async Task<IActionResult> Edit(Employee employee)
    {
        _context.Update(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        _context.Employees.Remove(emp);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
}
