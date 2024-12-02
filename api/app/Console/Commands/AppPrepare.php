<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;
use Illuminate\Support\Facades\Artisan;

class AppPrepare extends Command
{
    protected $signature = 'app:prepare';

    protected $description = 'Prepare the application for running';

    public function handle()
    {
        $this->info("Running app preparation for running");

        // Если существует .env — предложить скопировать примерную конфигурацию
        if (!file_exists(base_path('.env'))) {
            if ($this->confirm('Do you want to create .env file from .env.example?', true)) {
                copy(base_path('.env.example'), base_path('.env'));
                $this->info('.env file created');
            }
            else {
              $this->info('Without .env no app running');
            }
        }
        else $this->info('.env file exists');

        // Запрос выбор режима
        $mode = $this->choice(
            'Select the environment mode',
            ['production', 'development'],
            1
        );

        // Запись режима в env
        if ($mode == 'development') {
            envWrite('APP_ENV', 'local');
            envWrite('APP_DEBUG', 'true');
        }
        if ($mode == 'production') {
            envWrite('APP_ENV', 'production');
            envWrite('APP_DEBUG', 'false');
        }

        // Генерация ключа приложения
        if ($this->confirm('Do you want to generate the application key?', true))
            Artisan::call('key:generate');

        // Выполнение миграций
        if ($this->confirm('Do you want to run migrations?', true))
            Artisan::call('migrate');

        // Выполнение обязательных заполнителей
        if ($this->confirm('Do you want to run required database seeder?', true))
            Artisan::call('db:seed');

        // Если выбран development — предложить запустить фабрики (рандомные данные для тестирования)
        if ($mode === 'development') {
            if ($this->confirm('Do you want to run factories?', true))
                Artisan::call('db:seed --class=DatabaseSeeder');
        }
    }
}
