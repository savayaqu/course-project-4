<?php

namespace App\Http\Controllers\Api;

use App\Cacheables\SpaceInfo;
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
            'role', 'warnings',
            'complaintsFrom', 'complaintsFrom.type', 'complaintsFrom.album',
            'complaintsFrom.picture', 'complaintsFrom.aboutUser',
        ])->loadCount([
            'tags', 'albums', 'pictures', 'albumsViaAccess', 'complaintsFrom',
            'complaintsFrom as complaints_from_accepted_count' => fn ($query) => $query->where('status', 1),
        ]);

        $quotaTotal = $user->quotaTotal();
        $quotaUsed = $user->quotaUsed();

        return response()->json([
            'user' => UserResource::make($user),
            'quota' => [
                'total' => $quotaTotal,
                'used'  => $quotaUsed,
                'free'  => $quotaTotal - $quotaUsed,
            ],
        ]);
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
