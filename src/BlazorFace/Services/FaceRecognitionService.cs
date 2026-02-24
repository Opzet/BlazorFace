// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BlazorFace.Models;
using FaceAiSharp;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BlazorFace.Services;

/// <summary>
/// Simplified and robust face recognition service with thread-safe single instances
/// </summary>
public class FaceRecognitionService
{
    private readonly IFaceDetectorWithLandmarks _detector;
    private readonly IFaceEmbeddingsGenerator _embeddingsGenerator;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly SemaphoreSlim _detectorLock = new(1, 1);
    private readonly SemaphoreSlim _recognizerLock = new(1, 1);
    private bool _isInitialized;

    public FaceRecognitionService(
        IFaceDetectorWithLandmarks detector,
        IFaceEmbeddingsGenerator embeddingsGenerator,
        IEmployeeRepository employeeRepo)
    {
        _employeeRepo = employeeRepo;

        try
        {
            Console.WriteLine("[FaceRecognitionService] Initializing with injected services...");

            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _embeddingsGenerator = embeddingsGenerator ?? throw new ArgumentNullException(nameof(embeddingsGenerator));

            _isInitialized = true;
            Console.WriteLine($"[FaceRecognitionService] ✅ Initialized successfully!");
            Console.WriteLine($"[FaceRecognitionService] Detector: {_detector.GetType().Name}");
            Console.WriteLine($"[FaceRecognitionService] Generator: {_embeddingsGenerator.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FaceRecognitionService] ❌ Initialization failed: {ex.Message}");
            Console.WriteLine($"[FaceRecognitionService] Stack: {ex.StackTrace}");
            _isInitialized = false;
            throw;
        }
    }

    public async Task<FaceDetectionResult?> DetectFaceAsync(Image<Rgb24> image)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[DetectFace] ❌ Service not initialized!");
            return null;
        }

        await _detectorLock.WaitAsync();
        try
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime:HH:mm:ss.fff}] 🔍 Detecting face in {image.Width}x{image.Height} image");

            // Perform detection
            var detections = _detector.DetectFaces(image);

            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ✅ Detection completed in {elapsed:F0}ms - Found {detections.Count} face(s)");

            var first = detections.FirstOrDefault();
            if (first == null || first.Landmarks == null || first.Landmarks.Count == 0)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ⚠️ No valid face with landmarks detected");
                return null;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ✓ Face detected: {first.Landmarks.Count} landmarks, confidence: {first.Confidence:F3}");

            return new FaceDetectionResult
            {
                Landmarks = first.Landmarks.ToArray(),
                Confidence = first.Confidence ?? 0f,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ❌ Detection error: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Stack: {ex.StackTrace}");
            return null;
        }
        finally
        {
            _detectorLock.Release();
        }
    }

    public async Task<RecognitionResult> RecognizeAsync(Image<Rgb24> image, PointF[] landmarks, double threshold = 0.42)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[Recognize] ❌ Service not initialized!");
            return new RecognitionResult { Success = false, Embedding = Array.Empty<float>() };
        }

        await _recognizerLock.WaitAsync();
        try
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"[{startTime:HH:mm:ss.fff}] 🧠 Starting recognition (threshold: {threshold:P0})");

            // Clone and align face
            using var alignedImage = image.Clone();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 📐 Aligning face with {landmarks.Length} landmarks");
            _embeddingsGenerator.AlignFaceUsingLandmarks(alignedImage, landmarks);

            // Generate embedding
            var embStartTime = DateTime.Now;
            var embedding = _embeddingsGenerator.GenerateEmbedding(alignedImage);
            var embElapsed = (DateTime.Now - embStartTime).TotalMilliseconds;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 🔢 Generated {embedding.Length}D embedding in {embElapsed:F0}ms");

            // Find match
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 🔎 Searching employee database...");
            var matchedEmployee = await _employeeRepo.FindEmployeeByFaceAsync(embedding, threshold);

            if (matchedEmployee != null)
            {
                var confidence = GeometryExtensions.Dot(embedding, matchedEmployee.FaceEmbedding);
                var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ✅ MATCH: {matchedEmployee.Name} ({matchedEmployee.EmployeeId}) - {confidence:P0} in {totalElapsed:F0}ms");

                return new RecognitionResult
                {
                    Success = true,
                    Employee = matchedEmployee,
                    Confidence = confidence,
                    Embedding = embedding,
                };
            }

            var totalElapsed2 = (DateTime.Now - startTime).TotalMilliseconds;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ❌ NO MATCH: Below {threshold:P0} in {totalElapsed2:F0}ms");

            return new RecognitionResult
            {
                Success = false,
                Embedding = embedding,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ❌ Recognition error: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Stack: {ex.StackTrace}");
            return new RecognitionResult { Success = false, Embedding = Array.Empty<float>() };
        }
        finally
        {
            _recognizerLock.Release();
        }
    }
}

public class FaceDetectionResult
{
    public required PointF[] Landmarks { get; set; }
    public float Confidence { get; set; }
}

public class RecognitionResult
{
    public bool Success { get; set; }
    public Employee? Employee { get; set; }
    public double Confidence { get; set; }
    public required float[] Embedding { get; set; }
}
