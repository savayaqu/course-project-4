<?php

use App\Http\Controllers\Api\AccessController;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\InvitationController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\TagController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\WarningController;
use App\Http\Middleware\CheckAlbumAccess;
use App\Http\Middleware\CheckRole;
use App\Http\Middleware\SignCheck;
use Illuminate\Support\Facades\Route;

Route
::controller(AuthController::class)
->group(function ($auth) {                                          // [АВТОРИЗАЦИЯ]
    $auth->post('register', 'register');                            // Регистрация
    $auth->post('login'   , 'login');                               // Авторизация
    $auth->post('logout'  , 'logout')->middleware('auth:sanctum');  // Выход (удаление токена)
});

Route
::middleware('auth:sanctum')
->group(function ($authorized) { // [АВТОРИЗОВАННЫЕ ФУНКЦИИ]
    $authorized
    ->controller(AlbumController::class)
    ->prefix('albums')
    ->group(function ($albums) {        // [АЛЬБОМЫ]
        $albums->post('', 'create');    // Создание ЛИЧНОГО альбома
        $albums->get ('', 'index');     // Просмотр всех ЛИЧНЫХ и ДОСТУПНЫХ ЧУЖИХ альбомов
        $albums
        ->prefix('{album}')
        ->middleware(CheckAlbumAccess::class)
        ->group(function ($album) {         // [АЛЬБОМ]
            $album->get   ('', 'show');     // Просмотр информации об альбоме
            $album->post  ('', 'update');   // Изменение информации об СВОЁМ альбоме
            $album->delete('', 'destroy');  // Удаления СВОЕГО альбома и всё связанное с ним (в т.ч. и файлов)
            $album->delete('accesses/{user?}', [    AccessController::class, 'destroy'      ]);  // Убрать доступ у пользователя со СВОЕГО альбома / у СЕБЯ с ЧУЖОГО альбома
            $album->post  ('invite'          , [InvitationController::class, 'create'       ]);  // Генерировать код приглашения на СВОЙ альбом
            $album->post  ('complaint'       , [ ComplaintController::class, 'createToAlbum'])   // Создание жалобы на ЧУЖОЙ альбом
                  ->withoutMiddleware(CheckAlbumAccess::class);
            $album
            ->prefix('pictures')
            ->controller(PictureController::class)
            ->group(function ($albumPictures) {     // [КАРТИНКИ В АЛЬБОМЕ]
                $albumPictures->get ('', 'index');  // Список картинок в альбоме (с выдачей сигнатуры доступа)
                $albumPictures->post('', 'create'); // Загрузка картинок на сервер
                $albumPictures
                ->prefix('{picture}')
                ->group(function ($picture) {           // [КАРТИНКА]
                    $picture->get   ('', 'info');       // Получение информации об картинке
                    $picture->delete('', 'destroy');    // Удаление СВОЕЙ картинки
                    $picture->post('complaint', [ComplaintController::class, 'createToPicture']) // Создание жалобы на ЧУЖУЮ картинку
                            ->withoutMiddleware(CheckAlbumAccess::class);
                    $picture
                    ->withoutMiddleware(['auth:sanctum', CheckAlbumAccess::class])
                    ->middleware(SignCheck::class)
                    ->group(function ($pictureBySign) {                           // [ФАЙЛЫ КАРТИНКИ ПО СИГНАТУРЕ]
                        $pictureBySign->get('download'          , 'download');    // Скачивание картинки
                        $pictureBySign->get('original'          , 'original');    // Отображение картинки
                        $pictureBySign->get('thumb/{orient}{px}', 'thumbnail')    // Отображение превью картинки заданного размера
                            ->name('thumbnail')
                            ->where('orient' , '[qwhQWH]')
                            ->where('px', '[0-9]+')
                            ->withoutMiddleware("throttle:api");
                    });
                    $picture
                    ->prefix('tags')
                    ->controller(TagController::class)
                    ->group(function ($pictureTags) {                       // [ТЕГИ НА КАРТИНКЕ]
                        $pictureTags->post  ('{tag}', 'attachToPicture');   // Прикрепление тега к СВОЕЙ картинке
                        $pictureTags->delete('{tag}', 'detachToPicture');   // Открепление тега от СВОЕЙ картинки
                    });
                });
            });
        });
    });
    $authorized
    ->controller(InvitationController::class)
    ->prefix('invitation/{invitation}')
    ->group(function ($invitations) {           // [ПРИГЛАШЕНИЯ]
        $invitations->get('album', 'album');    // Просмотр содержимого альбома по приглашению
        $invitations->get('join' , 'join');     // Присоединиться к альбому (добавление доступа)
        $invitations->delete(''  , 'destroy');  // Удалить СВОЙ код приглашения
    });
    $authorized
    ->controller(AccessController::class)
    ->prefix('accesses')
    ->group(function ($accesses) {    // [ДОСТУПЫ]
        $accesses->get('', 'index');  // Список выданных доступов и приглашений
    });
    $authorized
    ->prefix('tags')
    ->controller(TagController::class)
    ->group(function ($tags) {      // [ТЕГИ]
        $tags->post('', 'create');  // Создание ЛИЧНОГО тега
        $tags->get ('', 'index');   // Просмотр ЛИЧНЫХ тегов
        $tags
        ->prefix('{tag}')
        ->group(function ($tag) {           // [ТЕГ]
            $tag->get   ('', 'show');       // Просмотр информации о теге (прикреплённые картинки)
            $tag->post  ('', 'edit');       // Изменение ЛИЧНОГО тега
            $tag->delete('', 'destroy');    // Удаление ЛИЧНОГО тега
        });
    });
    $authorized
    ->prefix('complaints')
    ->controller(ComplaintController::class)
    ->group(function ($complaints) {                    // [ЖАЛОБЫ]
        $complaints->get('', 'all');                    // Просмотр всех жалоб
        $complaints->delete('{complaint}', 'destroy');  // Удаление своей жалобы
    });
    $authorized
    ->prefix('users')
    ->controller(UserController::class)
    ->group(function ($users) {                                             // [ПОЛЬЗОВАТЕЛИ]
        $users->get ('', 'index')->middleware(CheckRole::class . ':admin'); // Список пользователей
        $users->get ('me', 'self');                                         // Получение СЕБЯ
        $users->post('me', 'selfEdit');                                     // Редактирование СЕБЯ
        $users
        ->prefix('{user}')
        ->middleware(CheckRole::class . ':admin')
        ->group(function ($user) {       // [ПОЛЬЗОВАТЕЛЬ]
            $user->get ('', 'show');     // Получение пользователя
            $user->post('', 'edit');     // Редактирование пользователя
            $user
            ->prefix('warnings')
            ->controller(WarningController::class)
            ->group(function ($warnings) {                  // [ПРЕДУПРЕЖДЕНИЯ]
                $warnings->post  (''         , 'create');   // Создание предупреждения
                $warnings->delete('{warning}', 'destroy');  // Удаление предупреждения
            });
        });

    });

    // TODO: Общая информация / настройки (разрешённые размеры превью, возможные типы жалоб, ?размер хранилища...) — SettingsController
});
