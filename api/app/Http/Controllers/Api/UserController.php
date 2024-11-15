<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UserSelfUpdateRequest;
use App\Http\Requests\Api\User\UserUpdateRequest;
use App\Http\Resources\UserResource;
use App\Models\User;
use Illuminate\Support\Facades\Auth;

class UserController extends Controller
{
    public function index()
    {
        $users = User::with([
            'role', 'warnings', 'complaintsAbout', /**/ 'albumsViaAccess', 'tags', 'complaintsFrom', 'albums', 'pictures',
        ])->withCount([
            'albumsViaAccess', 'tags', 'complaintsFrom', 'albums', 'pictures',
        ])->get();

        return response(['users' => UserResource::collection($users)]);
    }

    public function show(User $user)
    {
        $user->load([
            'role', 'warnings', 'complaintsAbout', 'complaintsFrom',
        ])->loadCount([
            'albums',  'pictures'
        ]);
        return response(['user' => UserResource::make($user)]);
    }

    public function showSelf()
    {
        $user = Auth::user();
        $user->load([
            'role', 'warnings', 'complaintsFrom',
        ])->loadCount([
            'albums', 'pictures', 'complaintsFrom',
        ]);
        return response(['user' => UserResource::make($user)]);
    }

    public function edit(UserUpdateRequest $request, User $user)
    {
        $user->update($request->validated());
        return response(['user' => UserResource::make($user)]);
    }

    public function editSelf(UserSelfUpdateRequest $request)
    {
        $user = Auth::user();
        $user->update($request->validated());
        return response(['user' => UserResource::make($user)]);
    }
}
