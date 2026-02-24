// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using BlazorFace.Models;
using FaceAiSharp.Extensions;

namespace BlazorFace.Services;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(string id);
    Task<Employee?> FindEmployeeByFaceAsync(float[] embedding, double threshold = 0.42);
    Task<Employee> AddEmployeeAsync(string employeeId, string name, float[] faceEmbedding);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(string id);
    Task<List<ClockEvent>> GetClockEventsAsync(DateTime? from = null, DateTime? to = null);
    Task AddClockEventAsync(ClockEvent clockEvent);
}

public class JsonFileEmployeeRepository : IEmployeeRepository
{
    private readonly string _employeesFilePath;
    private readonly string _clockEventsFilePath;
    private readonly SemaphoreSlim _employeesLock = new(1, 1);
    private readonly SemaphoreSlim _eventsLock = new(1, 1);

    public JsonFileEmployeeRepository()
    {
        var dataDir = Directory.GetCurrentDirectory();
        dataDir = Path.Combine(dataDir, "App_Data");
        Directory.CreateDirectory(dataDir);

        _employeesFilePath = Path.Combine(dataDir, "employees.json");
        _clockEventsFilePath = Path.Combine(dataDir, "clock_events.json");
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        await _employeesLock.WaitAsync();
        try
        {
            if (!File.Exists(_employeesFilePath))
                return new List<Employee>();

            var json = await File.ReadAllTextAsync(_employeesFilePath);
            return JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();
        }
        finally
        {
            _employeesLock.Release();
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(string id)
    {
        var employees = await GetAllEmployeesAsync();
        return employees.FirstOrDefault(e => e.Id == id);
    }

    public async Task<Employee?> FindEmployeeByFaceAsync(float[] embedding, double threshold = 0.42)
    {
        var employees = await GetAllEmployeesAsync();
        
        Employee? bestMatch = null;
        double bestSimilarity = 0;

        foreach (var employee in employees)
        {
            var similarity = GeometryExtensions.Dot(embedding, employee.FaceEmbedding);
            if (similarity >= threshold && similarity > bestSimilarity)
            {
                bestMatch = employee;
                bestSimilarity = similarity;
            }
        }

        return bestMatch;
    }

    public async Task<Employee> AddEmployeeAsync(string employeeId, string name, float[] faceEmbedding)
    {
        await _employeesLock.WaitAsync();
        try
        {
            var employees = await GetAllEmployeesAsync();
            
            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = employeeId,
                Name = name,
                FaceEmbedding = faceEmbedding,
                RegisteredAt = DateTime.UtcNow,
            };

            employees.Add(employee);
            await SaveEmployeesAsync(employees);
            return employee;
        }
        finally
        {
            _employeesLock.Release();
        }
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        await _employeesLock.WaitAsync();
        try
        {
            var employees = await GetAllEmployeesAsync();
            var index = employees.FindIndex(e => e.Id == employee.Id);
            
            if (index >= 0)
            {
                employees[index] = employee;
                await SaveEmployeesAsync(employees);
            }
        }
        finally
        {
            _employeesLock.Release();
        }
    }

    public async Task DeleteEmployeeAsync(string id)
    {
        await _employeesLock.WaitAsync();
        try
        {
            var employees = await GetAllEmployeesAsync();
            employees.RemoveAll(e => e.Id == id);
            await SaveEmployeesAsync(employees);
        }
        finally
        {
            _employeesLock.Release();
        }
    }

    public async Task<List<ClockEvent>> GetClockEventsAsync(DateTime? from = null, DateTime? to = null)
    {
        await _eventsLock.WaitAsync();
        try
        {
            if (!File.Exists(_clockEventsFilePath))
                return new List<ClockEvent>();

            var json = await File.ReadAllTextAsync(_clockEventsFilePath);
            var events = JsonSerializer.Deserialize<List<ClockEvent>>(json) ?? new List<ClockEvent>();

            if (from.HasValue)
                events = events.Where(e => e.Timestamp >= from.Value).ToList();
            
            if (to.HasValue)
                events = events.Where(e => e.Timestamp <= to.Value).ToList();

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }
        finally
        {
            _eventsLock.Release();
        }
    }

    public async Task AddClockEventAsync(ClockEvent clockEvent)
    {
        await _eventsLock.WaitAsync();
        try
        {
            var events = await GetClockEventsAsync();
            events.Add(clockEvent);
            await SaveClockEventsAsync(events);
        }
        finally
        {
            _eventsLock.Release();
        }
    }

    private async Task SaveEmployeesAsync(List<Employee> employees)
    {
        var json = JsonSerializer.Serialize(employees, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_employeesFilePath, json);
    }

    private async Task SaveClockEventsAsync(List<ClockEvent> events)
    {
        var json = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_clockEventsFilePath, json);
    }
}
