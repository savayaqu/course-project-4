<?php

use App\Http\Controllers\Api\AuthController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use \App\Http\Controllers\Api\AlbumController;
use \App\Http\Controllers\Api\PictureController;
use \App\Http\Middleware\CheckRole;

Route::controller(AuthController::class)->group(function ($users) {
   $users->post('/register', 'register');
   $users->post('/login', 'login');
   $users->middleware('auth:sanctum')->post('/logout', 'logout');
});


Route::controller(AlbumController::class)->group(function ($albums) {
    //Создание альбома
    $albums->middleware('auth:sanctum')->post('/albums', 'create');
    //Удаление альбома со всем содержимым
    $albums->middleware('auth:sanctum')->delete('/albums/{album}', 'destroy');
    //Просмотр альбомов
    $albums->middleware('auth:sanctum')->get('/albums', 'index');
    //Просмотр картинок в папке
    $albums->middleware('auth:sanctum')->get('/albums/{album}/images', 'showPictures');



    //реализовать для админа$albums->middleware('auth:sanctum', CheckRole::class.':admin')->delete('/albums/{album}', 'destroy');

});

Route::controller(PictureController::class)->group(function ($pictures) {
   $pictures->middleware('auth:sanctum')->post('/albums/{album}/images', 'create');
});



