using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

[TestFixture]
public class ServiceContactServiceTest
{
    private AppDbContext _context;
    private ServiceContactRepository _repository;
    private ServiceContactService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + System.Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new ServiceContactRepository(_context);
        _service = new ServiceContactService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllContactsAsync_ContactosExistentes_RetornaListaDeContactos()
    {
        // HU-06 - Visualizar contactos de servicios del hotel
        // CA 1: Dado que existen contactos cargados en la base de datos, cuando el usuario
        // ingrese a la página de servicios, entonces el sistema debe mostrar la lista de
        // contactos disponibles.

        // Arrange
        var contact1 = new ServiceContact { Id = 1, ServiceName = "Lavandería", Responsible = "Maria Lopez", Phone = "77777777" };
        var contact2 = new ServiceContact { Id = 2, ServiceName = "Mantenimiento", Responsible = "Carlos Perez", Phone = "88888888" };
        await _context.ServiceContacts.AddRangeAsync(contact1, contact2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllContactsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].ServiceName, Is.EqualTo("Lavandería"));
        Assert.That(result[1].ServiceName, Is.EqualTo("Mantenimiento"));
    }

    [Test]
    public async Task GetAllContactsAsync_DatosCargados_ContieneCamposPrincipales()
    {
        // HU-06 - Visualizar contactos de servicios del hotel
        // CA 2: Dado que cada servicio tiene información registrada, cuando se visualice en
        // la página, entonces deben mostrarse al menos el nombre del servicio,
        // encargado y teléfono.

        // Arrange
        var contact = new ServiceContact { Id = 1, ServiceName = "Lavandería", Responsible = "Maria Lopez", Phone = "77777777" };
        await _context.ServiceContacts.AddAsync(contact);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllContactsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result[0].ServiceName, Is.EqualTo("Lavandería"));
        Assert.That(result[0].Responsible, Is.EqualTo("Maria Lopez"));
        Assert.That(result[0].Phone, Is.EqualTo("77777777"));
    }
}
