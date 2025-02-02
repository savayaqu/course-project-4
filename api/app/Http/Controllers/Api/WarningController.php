<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Resources\WarningResource;
use App\Models\User;
use App\Models\Warning;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Storage;

class WarningController extends Controller
{
    public function store(Request $request, User $user): JsonResponse
    {
        if(Warning::where('user_id', $user->id)->count() >= config('settings.warning_limit_for_ban'))
        {
            $user->ban();
            throw new ApiException("User $user->username already banned", 409);
        }
        $warning = Warning::create([
            'user_id' => $user->id,
            'comment' => $request->comment
        ]);
        return response()->json(['warning' => WarningResource::make($warning)], 201);
    }

    public function destroy(User $user, Warning $warning): JsonResponse
    {
        $warning->delete();
        return response()->json(null, 204);
    }
}
