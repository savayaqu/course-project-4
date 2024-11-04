<?php

namespace App\Http\Controllers;

use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class UserController extends Controller
{
    public function show($userId)
    {
        return response()->json(User::findOrFail($userId), 200);
    }
    public function edit(Request $request, $userId)
    {
        $data = array_filter($request->all(), function ($value) {
            return !is_null($value) && $value !== '';
        });

        User::findOrFail($userId)->update($data);
        return response()->json(User::findOrFail($userId), 200);
    }
    public function index()
    {
        return response()->json(User::all())->setStatusCode(200);
    }
    public function self()
    {
        return response()->json(Auth::user())->setStatusCode(200);
    }
    public function selfEdit(Request $request)
    {
        $data = array_filter($request->all(), function ($value) {
            return !is_null($value) && $value !== '';
        });
        Auth::user()->update($data);
        return response()->json(Auth::user())->setStatusCode(200);
    }
}
