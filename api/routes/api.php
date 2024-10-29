<?php

use App\Http\Controllers\Api\AuthController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\TagController;

use App\Http\Middleware\CheckRole;

Route::controller(AuthController::class)->group(function ($auth) {
    // Авторизация
    $auth->post('register', 'register');
    $auth->post('login'   , 'login');
    $auth->post('logout'  , 'logout')->middleware('auth:sanctum');
});

Route
::middleware('auth:sanctum')
->group(function ($authorized) {
    // Авторизованные функции
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
            $album->get   ('', 'show'); // TODO: Тут выводить картинки вместе с инфой об альбоме
            $album->post  ('', 'edit');
            $album->delete('', 'destroy');
          /*$album // TODO: Удалить, если не делать доступы по логинам (хочется таблицу приглашений, если делать)
            ->controller(AccessController::class)
            ->prefix('accesses')
            ->group(function ($accesses) {
                $accesses->post  (''        , 'create');
                $accesses->delete('{access}', 'destroy');
            });*/
            $album->post('complaint', [ComplaintsController::class, 'createToAlbum']); // Жалоба на альбом
            $album
          //->prefix('pictures') // TODO: Удалить, если выводить в информации об альбоме, но потеря пагинации
            ->controller(PictureController::class)
            ->group(function ($albumPictures) {
                // Картинки в альбоме
              //$albumPictures->get ('', 'index'); // TODO: Удалить, если выводить в информации об альбоме, но потеря пагинации
                $albumPictures->post('', 'create');
                $albumPictures
                ->prefix('{picture}')
                ->group(function ($picture) {
                    // Картинка
                    $picture->get (''        , 'info');
                    $picture->get ('original', 'original');
                    $picture->get ('download', 'download');
                    $picture->post('complaint', [ComplaintsController::class, 'createToPicture']);
                    $picture
                    ->prefix('tags')
                    ->controller(TagController::class)
                    ->group(function ($pictureTags) {
                        // Управление тегами на картинке
                        $pictureTags->post  (''    , 'attachToPicture');
                        $pictureTags->delete('{id}', 'detachToPicture');
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
    // TODO: Доступы
    // TODO: Жалобы
    // TODO: Пользователи
    // TODO: Общая информация / настройки (разрешённые размеры превью, возможные типы жалоб, ?размер хранилища...)
});
