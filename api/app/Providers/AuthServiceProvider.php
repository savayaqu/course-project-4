<?php
namespace App\Providers;

use Illuminate\Foundation\Support\Providers\AuthServiceProvider as ServiceProvider;

class AuthServiceProvider extends ServiceProvider
{
    /**
     * Сопоставление моделей и политик.
     *
     * @var array<class-string, class-string>
     */
    protected $policies = [
    ];

    /**
     * Регистрация любых служб аутентификации и авторизации.
     */
    public function boot()
    {
    }
}
