<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UserSelfUpdateRequest;
use App\Http\Requests\Api\User\UserUpdateRequest;
use App\Http\Resources\UserResource;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;

class UserController extends Controller
{
    public function index(): JsonResponse
    {
        $users = User::with([
            'role', 'warnings',
        ])->withCount([
            'tags', 'albums', 'pictures', 'albumsViaAccess', 'complaintsFrom', 'complaintsAbout'
        ])->get();

        return response()->json(['users' => UserResource::collection($users)]);
    }

    public function show(User $user): JsonResponse
    {
        $user->load([
            'role', 'warnings', 'complaintsAbout', 'complaintsFrom',
        ])->loadCount([
            'tags', 'albums',  'pictures', 'albumsViaAccess',
        ]);
        return response()->json(['user' => UserResource::make($user)]);
    }

    public function showSelf(): JsonResponse
    {
        $user = Auth::user();
        $user->load([
            'role', 'warnings', 'complaintsFrom',
        ])->loadCount([
            'tags', 'albums', 'pictures', 'albumsViaAccess',
        ]);
        return response()->json(['user' => UserResource::make($user)]);
    }

    public function update(UserUpdateRequest $request, User $user): JsonResponse
    {
        if($request->is_banned == 1)
        {
            $user->ban();
        }
        $user->update($request->validated());
        return response()->json(['user' => UserResource::make($user)]);
    }

    public function updateSelf(UserSelfUpdateRequest $request): JsonResponse
    {
        $user = Auth::user();
        $user->update($request->validated());
        return response()->json(['user' => UserResource::make($user)]);
    }
}
