<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\ComplaintType\ComplaintTypeCreateRequest;
use App\Http\Requests\Api\ComplaintType\ComplaintTypeUpdateRequest;
use App\Models\ComplaintType;
use Illuminate\Http\JsonResponse;

class ComplaintTypeController extends Controller
{
    public function index(): JsonResponse
    {
        $complaints = ComplaintType::all();
        return response()->json($complaints);
    }

    public function store(ComplaintTypeCreateRequest $request): JsonResponse
    {
        $complaintType = ComplaintType::create($request->validated());
        return response()->json($complaintType, 201);
    }

    public function update(ComplaintType $complaintType, ComplaintTypeUpdateRequest $request): JsonResponse
    {
        $complaintType->update($request->validated());
        return response()->json($complaintType);
    }

    public function destroy(ComplaintType $complaintType): JsonResponse
    {
        $complaintType->delete();
        return response()->json(null, 204);
    }
}
