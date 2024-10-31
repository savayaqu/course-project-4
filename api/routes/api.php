<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\InvitationController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\TagController;
use App\Http\Controllers\Api\AccessController;

use App\Http\Middleware\CheckRole;

Route::controller(AuthController::class)->group(function ($auth) {
    // Авторизация
    $auth->post('register', 'register');
    $auth->post('login'   , 'login');
    $auth->post('logout'  , 'logout')->middleware('auth:sanctum');
});
// Работа с картинками с sign
Route::controller(PictureController::class)->prefix('albums/{album}/pictures/{picture}')->group(function ($pictures) {
    $pictures->get('download', 'download');
    $pictures->get('original', 'original');
    $pictures->get('thumb/{size}', 'thumbnail');
});

Route
::middleware('auth:sanctum')
->group(function ($authorized) {
    // Авторизованные функции
    //Приглашения
    $authorized
        ->controller(InvitationController::class)
        ->prefix('invitation/{code}')
        ->group(function ($invitations) {
            $invitations->get('join', 'join'); //Присоединиться к альбому
            $invitations->get('album', 'album'); //Просмотр содержимого альбома по приглосу
        });
    //Доступы
    $authorized
        ->controller(AccessController::class)
        ->group(function ($accesses) {
            $accesses->get('accesses', 'all'); // Список выданных доступов (и приглашений?)
            $accesses->delete('/albums/{album}/accesses/{user}', 'destroy'); // Убрать доступ у пользователя
        });
    $authorized
    ->controller(AlbumController::class)
    ->prefix('albums')
    ->group(function ($albums) {
        // Альбомы
        $albums->post('', 'create');
        $albums->get ('', 'index');
        $albums
        ->prefix('{album}')
        ->group(function ($album) {
            // Альбом
            $album->get   ('', 'show');
            $album->post  ('', 'edit');
            $album->delete('', 'destroy');
            $album->post('invite'   , [InvitationController::class, 'create'       ]); // Код приглашения
            $album->post('complaint', [ ComplaintController::class, 'createToAlbum']); // Жалоба на альбом
            $album
            ->prefix('pictures')
            ->controller(PictureController::class)
            ->group(function ($albumPictures) {
                // Картинки в альбоме
                $albumPictures->get ('', 'index');
                $albumPictures->post('', 'create');
                $albumPictures
                ->prefix('{picture}')
                ->group(function ($picture) {
                    // Картинка
                    $picture->get (  ''   , 'info');
                    $picture->delete('' , 'destroy');
                    $picture->post('complaint', [ComplaintController::class, 'createToPicture']);
                    $picture
                    ->prefix('tags')
                    ->controller(TagController::class)
                    ->group(function ($pictureTags) {
                        // Управление тегами на картинке
                        $pictureTags->post  ('{tag}', 'attachToPicture');
                        $pictureTags->delete('{tag}', 'detachToPicture');
                    });
                });
            });
        });
    });
    $authorized
    ->prefix('tags')
    ->controller(TagController::class)
    ->group(function ($tags) {
        // Теги
        $tags->post('', 'create');
        $tags->get ('', 'index');
        $tags->prefix('{tag}')->group(function ($tag) {
            // Тег
            $tag->get   ('', 'show');
            $tag->post  ('', 'edit');
            $tag->delete('', 'destroy');
        });
    });
// TODO: РАЗРАБОТАТЬ middleware ДЛЯ АЛЬБОМОВ ПО ДОСТУПУ

    // TODO: Жалобы       —  ComplaintController
    // TODO: Пользователи —       UserController
    // TODO: Общая информация / настройки (разрешённые размеры превью, возможные типы жалоб, ?размер хранилища...) — SettingsController
});
