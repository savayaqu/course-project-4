<?php

// app/Providers/AuthServiceProvider.php

namespace App\Providers;

use App\Models\Picture;
use Illuminate\Foundation\Support\Providers\AuthServiceProvider as ServiceProvider;
use Illuminate\Support\Facades\Gate;
use App\Models\Album;
use App\Policies\AlbumPolicy;

class AuthServiceProvider extends ServiceProvider
{
    /**
     * Сопоставление моделей и политик.
     *
     * @var array<class-string, class-string>
     */
    protected $policies = [
        Album::class => AlbumPolicy::class, // Регистрация политики для Album
    ];

    /**
     * Регистрация любых служб аутентификации и авторизации.
     */
    public function boot()
    {
        $this->registerPolicies(); // Регистрируем политики

        // Определяем доступ через Gate
        Gate::define('view-album-if', [AlbumPolicy::class, 'view']);
        // Альбомы только для чтения
    }
}
