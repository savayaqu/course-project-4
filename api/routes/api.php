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

Route::controller(AlbumController::class)->prefix('albums')->middleware('auth:sanctum')->group(function ($albums) {
    //Создание альбома
    $albums->post( '','create');
    //Удаление альбома со всем содержимым
    $albums->delete('/{album}', 'destroy');
    //Просмотр альбомов
    $albums->get('', 'index');
    //Просмотр картинок в папке
    $albums->get('/{album}/images', 'showPictures');
    //реализовать для админа
    //$albums->middleware(CheckRole::class.':admin')->delete('/{album}', 'destroy');

});

Route::controller(PictureController::class)->middleware('auth:sanctum')->group(function ($pictures) {
    //Создание картинок в альбом
   $pictures->post('/albums/{album}/images', 'create');
   //Скачивание картинки
    $pictures->get('/albums/{album}/images/{image}/download', 'download');
});



