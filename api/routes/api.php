<?php

use App\Http\Controllers\Api\AuthController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use \App\Http\Controllers\Api\AlbumController;
use \App\Http\Controllers\Api\PictureController;

Route::controller(AuthController::class)->group(function ($users) {
   $users->post('/register', 'register');
   $users->post('/login', 'login');
   $users->middleware('auth:sanctum')->post('/logout', 'logout');
});


Route::controller(AlbumController::class)->group(function ($albums) {
    $albums->post('/create', 'create');
});

Route::controller(PictureController::class)->group(function ($pictures) {
   $pictures->middleware('auth:sanctum')->post('/albums/{album}/images', 'create');
});



