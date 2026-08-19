using System.Text.Json;
using TransversalUtilityServices.Services;
using TransversalUtilityServiceDemo.Beans;
var configPath = @"D:\workspace\utility packages\Algorithms\SearchAlgorithm\Transversal\TransversalUtilityServiceDemo\TransversalUtilityServiceDemo\jobs-config.json";

// 讀取檔案內容
string jsonString = File.ReadAllText(configPath);

// 將 JSON 轉為物件
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
JobConfig configObj = JsonSerializer.Deserialize<JobConfig>(jsonString, options);

ITransversalService transversalService = new DFSTransversalService();
transversalService.Transverse(
    configObj, (obj) => Console.WriteLine($"掃描到物件: {obj.GetType().Name}")
);