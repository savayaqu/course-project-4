<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\InvitationController;
use App\Http\Controllers\UserController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AlbumController;
use App\Http\Controllers\Api\PictureController;
use App\Http\Controllers\Api\ComplaintController;
use App\Http\Controllers\Api\TagController;
use App\Http\Controllers\Api\AccessController;
use App\Http\Controllers\Api\WarningController;
use App\Http\Middleware\CheckAlbumAccess;
use App\Http\Middleware\CheckRole;

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
        $albums->post('', 'create');    // Создание личного альбома
        $albums->get ('', 'index');     // Просмотр всех ЛИЧНЫХ и ДОСТУПНЫХ ЧУЖИХ альбомов
        $albums
        ->prefix('{album}')
        ->middleware(CheckAlbumAccess::class) // TODO: РАЗРАБОТАТЬ middleware ДЛЯ АЛЬБОМОВ ПО ДОСТУПУ (мб только для GET запросов, т.к. DEL и POST чисто для действий со СВОИМИ альбомами (кроме жалоб))
        ->group(function ($album) {         // [АЛЬБОМ]
            $album->get   ('', 'show');     // Просмотр информации об альбоме
            $album->post  ('', 'edit');     // Изменение информации об СВОЁМ альбоме
            $album->delete('', 'destroy');  // Удаления СВОЕГО альбома и всё связанное с ним (в т.ч. и файлов)
            $album->delete('accesses/{user}', [    AccessController::class, 'destroy'      ]);  // Убрать доступ у пользователя со СВОЕГО альбома / у СЕБЯ с ЧУЖОГО альбома
            $album->delete('invite'         , [InvitationController::class, 'destroy'      ]);  // Удалить код приглашения на СВОЁМ альбоме
            $album->post  ('invite'         , [InvitationController::class, 'create'       ]);  // Генерировать код приглашения на СВОЙ альбом
            $album->post  ('complaint'      , [ ComplaintController::class, 'createToAlbum']);  // Создание жалоба на ЧУЖОЙ альбом
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
                    $picture->post('complaint', [ComplaintController::class, 'createToPicture']);   // Создание жалобы на ЧУЖУЮ картинку
                    $picture
                    ->withoutMiddleware('auth:sanctum')
                  //->middleware('sign:check') // TODO: вынести проверку сигнатур в middleware
                    ->group(function ($pictureBySign) {                     // [ФАЙЛЫ КАРТИНКИ ПО СИГНАТУРЕ]
                        $pictureBySign->get('download'    , 'download');    // Скачивание картинки
                        $pictureBySign->get('original'    , 'original');    // Отображение картинки
                        $pictureBySign->get('thumb/{size}', 'thumbnail');   // Отображение превью картинки заданного размера
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
    ->prefix('invitation/{code}')
    ->group(function ($invitations) {           // [ПРИГЛАШЕНИЯ]
        $invitations->get('album', 'album');    // Просмотр содержимого альбома по приглашению
        $invitations->get('join' , 'join');     // Присоединиться к альбому (добавление доступа)
    });
    $authorized
    ->controller(AccessController::class)
    ->prefix('accesses')
    ->group(function ($accesses) {  // [ДОСТУПЫ]
        $accesses->get('', 'all');  // Список выданных доступов (и приглашений?)
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
            $tag->post  ('', 'edit');       // Изменение тега
            $tag->delete('', 'destroy');    // Удаление тега
        });
    });
    $authorized
        ->prefix('complaints')
        ->controller(ComplaintController::class)
        ->group(function ($complaints) {                    //  [ЖАЛОБЫ]
            $complaints->get('', 'all');                    // Просмотр всех жалоб
            $complaints->delete('{complaint}', 'destroy');  //  Удаление своей жалобы
        });
    $authorized
        ->prefix('users')
        ->controller(UserController::class)
        ->group(function ($users) {            // [ПОЛЬЗОВАТЕЛИ]
            $users->middleware(CheckRole::class . ':admin')->get('', 'index');   // Список пользователей
            $users->get('me', 'self');         // Получение себя
            $users->post('me', 'selfEdit');  // Редактирование себя
            $users
                ->prefix('{user}')
                ->middleware(CheckRole::class . ':admin')
                ->group(function ($user) {
                   $user->get   ('', 'show');  // Получение пользователя
                   $user->post  ('', 'edit');  // Редактирование пользователя
                    $user
                        ->prefix('warnings')
                        ->controller(WarningController::class)
                        ->group(function ($warnings) {  // [ПРЕДУПРЕЖДЕНИЯ]
                            $warnings->post('', 'create');              //Создание предупреждения
                            $warnings->delete('{warning}', 'destroy');  //Удаление предупреждения
                        });
                });

        });

    // TODO: Общая информация / настройки (разрешённые размеры превью, возможные типы жалоб, ?размер хранилища...) — SettingsController
});
