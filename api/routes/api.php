<?php

use App\Http\Controllers\Api\AccessController;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\ComplaintTypeController;
use App\Http\Controllers\Api\InvitationController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\SettingsController;
use App\Http\Controllers\Api\TagController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\WarningController;
use App\Http\Middleware\CheckAlbumAccess;
use App\Http\Middleware\CheckRole;
use App\Http\Middleware\SanctumAuth;
use App\Http\Middleware\SignCheck;
use Illuminate\Support\Facades\Route;

Route
::controller(AuthController::class)
->group(function ($auth) {                  // [АВТОРИЗАЦИЯ]
    $auth->post('register', 'register');    // Регистрация
    $auth->post('login'   , 'login');       // Авторизация
    $auth->post('logout'  , 'logout')       // Выход (удаление токена)
        ->middleware(SanctumAuth::class);
});

Route
::middleware(SanctumAuth::class)
->group(function ($authorized) { // [АВТОРИЗОВАННЫЕ ФУНКЦИИ]
    $authorized
    ->controller(AlbumController::class)
    ->prefix('albums')
    ->group(function ($albums) {        // [АЛЬБОМЫ]
        $albums->post('', 'store');     // Создание ЛИЧНОГО альбома
        $albums->get ('', 'index');     // Просмотр всех ЛИЧНЫХ и ДОСТУПНЫХ ЧУЖИХ альбомов
        $albums
        ->prefix('{album}')
        ->middleware(CheckAlbumAccess::class)
        ->group(function ($album) {         // [АЛЬБОМ]
            $album->get   ('', 'show');     // Просмотр информации об альбоме
            $album->post  ('', 'update');   // Изменение информации об СВОЁМ альбоме
            $album->delete('', 'destroy');  // Удаления СВОЕГО альбома и всё связанное с ним (в т.ч. и файлов)
            $album->delete('accesses'       , [    AccessController::class, 'destroy'     ])    // Убрать доступ у пользователя у СЕБЯ с ЧУЖОГО альбома
                ->withoutMiddleware(CheckAlbumAccess::class);
            $album->delete('accesses/{user}', [    AccessController::class, 'destroy'     ]);   // Убрать доступ у пользователя со СВОЕГО альбома
            $album->post  ('invite'         , [InvitationController::class, 'store'       ]);   // Генерировать код приглашения на СВОЙ альбом
            $album->post  ('complaint'      , [ ComplaintController::class, 'storeToAlbum'])    // Создание жалобы на ЧУЖОЙ альбом
                ->withoutMiddleware(CheckAlbumAccess::class);
            $album
            ->prefix('pictures')
            ->controller(PictureController::class)
            ->group(function ($albumPictures) {     // [КАРТИНКИ В АЛЬБОМЕ]
                $albumPictures->get ('', 'index');  // Список картинок в альбоме (с выдачей сигнатуры доступа)
                $albumPictures->post('', 'store');  // Загрузка картинок на сервер
                $albumPictures
                ->prefix('{picture}')
                ->group(function ($picture) {           // [КАРТИНКА]
                    $picture->get   ('', 'info');       // Получение информации об картинке
                    $picture->delete('', 'destroy');    // Удаление СВОЕЙ картинки
                    $picture->post('complaint', [ComplaintController::class, 'storeToPicture']) // Создание жалобы на ЧУЖУЮ картинку
                            ->withoutMiddleware(CheckAlbumAccess::class);
                    $picture
                    ->withoutMiddleware([SanctumAuth::class, CheckAlbumAccess::class])
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
    ->group(function ($invitations) {           // [ПРИГЛАШЕНИЕ]
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
        $tags->post('', 'store');   // Создание ЛИЧНОГО тега
        $tags->get ('', 'index');   // Просмотр ЛИЧНЫХ тегов
        $tags
        ->prefix('{tag}')
        ->group(function ($tag) {           // [ТЕГ]
            $tag->get   ('', 'show');       // Просмотр информации о теге (прикреплённые картинки)
            $tag->post  ('', 'update');     // Изменение ЛИЧНОГО тега
            $tag->delete('', 'destroy');    // Удаление ЛИЧНОГО тега
        });
    });
    $authorized
    ->prefix('complaints')
    ->controller(ComplaintController::class)
    ->group(function ($complaints) {  // [ЖАЛОБЫ]
        $complaints
        ->prefix('types')
        ->controller(ComplaintTypeController::class)
        ->middleware(CheckRole::class . ':admin')
        ->group(function ($complaintTypes) {        // [ТИПЫ ЖАЛОБЫ]
            $complaintTypes->post('', 'store');     // Создание новых типов жалоб
            $complaintTypes->get ('', 'index')      // Просмотр всех типов жалоб
            ->withoutMiddleware(CheckRole::class . ':admin');
            $complaintTypes
            ->prefix('{complaintType}')
            ->group(function ($complaintType) {          // [ТИП ЖАЛОБЫ]
                $complaintType->post  ('', 'update');    // Редактирование типа жалоб
                $complaintType->delete('', 'destroy');   // Удаление типа жалоб
            });
        });
        $complaints->get('', 'index');  // Просмотр ВСЕХ жалоб (админ) / СВОИХ жалоб
        $complaints
        ->prefix('{complaint}')
        ->group(function ($complaint) {               // [ЖАЛОБА]
            $complaint->delete('', 'destroy');        // Удаление СВОЕЙ жалобы
            $complaint->post  ('', 'updateBatch')     // Изменение жалобы
                ->middleware(CheckRole::class . ':admin');
        });
    });
    $authorized
    ->prefix('users')
    ->controller(UserController::class)
    ->group(function ($users) {             // [ПОЛЬЗОВАТЕЛИ]
        $users->get ('me', 'showSelf');     // Получение СЕБЯ
        $users->post('me', 'updateSelf');   // Редактирование СЕБЯ
        $users->get ('', 'index')           // Список пользователей
            ->middleware(CheckRole::class . ':admin');
        $users
        ->prefix('{user}')
        ->middleware(CheckRole::class . ':admin')
        ->group(function ($user) {       // [ПОЛЬЗОВАТЕЛЬ]
            $user->get ('', 'show');     // Получение пользователя
            $user->post('', 'update');   // Редактирование пользователя
            $user
            ->prefix('warnings')
            ->controller(WarningController::class)
            ->group(function ($warnings) {                  // [ПРЕДУПРЕЖДЕНИЯ]
                $warnings->post  (''         , 'store');    // Создание предупреждения
                $warnings->delete('{warning}', 'destroy');  // Удаление предупреждения
            });
        });
    });
    $authorized
    ->controller(SettingsController::class)
    ->prefix('settings')
    ->middleware(CheckRole::class . ':admin')
    ->group(function ($settings) {      // [НАСТРОЙКИ]
        $settings->get ('', 'index');   // Получение всех настроек
        $settings->post('', 'edit');    // Изменение настроек
    });
});
Route::get('', [SettingsController::class, 'public'])
    ->middleware('cache.headers:public;max_age=2628000;etag'); // Публичные настройки

