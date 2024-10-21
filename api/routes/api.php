<?php

use App\Http\Controllers\Api\AuthController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\TagController;

use App\Http\Middleware\CheckRole;

Route::controller(AuthController::class)->group(function ($users) {
    //Регистрация
   $users->post('/register', 'register');
   //Вход
   $users->post('/login', 'login');
   //Выход
   $users->middleware('auth:sanctum')->post('/logout', 'logout');
});

Route::middleware('auth:sanctum')->group(function () {
    Route::controller(AlbumController::class)->prefix('albums')->group(function ($albums) {

        //Создание альбома
        $albums->post( '','create');
        //Удаление альбома со всем содержимым
        $albums->delete('/{album}', 'destroy');
        //Просмотр альбомов
        $albums->get('', 'index');
        //Просмотр картинок в папке
        $albums->get('/{album}', 'showPictures');

        //Нереализованный функционал:

        //Создание ссылки-приглашения на альбом
        $albums->post('{album}/access', 'createAccess');
        //Удаление ссылки-приглашения на альбом
        $albums->delete('{album}/access/{access}', 'destroy');
        $albums->middleware(CheckRole::class.':admin')->delete('/{album}', 'destroy');

    });




//--------------------------------------------------
//Всё что ниже не реализовано
    Route::controller(PictureController::class)->prefix('/{picture}')->group(function ($pictures) {
        //Добавление тега к картинки
        $pictures->post('', 'addTag');
        //Удаление тега с картинки
        $pictures->delete('', 'destroyTag');
    });
    Route::controller(ComplaintController::class)->prefix('complaints')->group(function ($complaints) {
        //Создание жалобы
        $complaints->post('', 'create');
        //Удаление жалобы (для админа)
        $complaints->middleware(CheckRole::class.':admin')->delete('/{complaint}', 'destroy');
    });
    Route::controller(TagController::class)->prefix('tags')->group(function ($tags) {
        //Создание тега
        $tags->post( '','create');
        //Редактирование тега
        $tags->post('/{tag}', 'edit');
        //Удаление тега
        $tags->delete('/{tag}', 'destroy');
    });
});






