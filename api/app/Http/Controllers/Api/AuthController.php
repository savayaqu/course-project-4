<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Auth\LoginRequest;
use App\Http\Requests\Api\Auth\RegisterRequest;
use App\Models\Role;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class AuthController extends Controller
{
    public function login(LoginRequest $request)
    {
        if (!Auth::attempt($request->only('login', 'password')))
            throw new ApiException('Invalid credentials', 401);

        $user = Auth::user();
        $token = $user->createToken('remember_token')->plainTextToken;
        return response([
            'success' => true,
            'token' => $token,
            'user' => $user,
        ]);
    }

    public function register(RegisterRequest $request)
    {
        $roleId = Role::firstOrCreate(['code' =>'user'])->id;
        $user = User::create([...$request->validated(), 'role_id' => $roleId]);
        $token = $user->createToken('remember_token')->plainTextToken;
        return response([
            'success' => true,
            'token' => $token,
            'user' => $user,
        ], 201);
    }

    public function logout(Request $request)
    {
        $request->user()->currentAccessToken()->delete();
        return response(null, 204);
    }
}
