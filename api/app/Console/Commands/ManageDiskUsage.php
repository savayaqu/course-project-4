<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Log;

class ManageDiskUsage extends Command
{
    /**
     * The name and signature of the console command.
     *
     * @var string
     */
    protected $signature = 'disk:manage';

    /**
     * The console command description.
     *
     * @var string
     */
    protected $description = 'Check disk usage';

    /**
     * Execute the console command.
     */
    public function handle()
    {
        Log::info("Checking disk usage");
        // Получить общее и свободное место на диске
        $totalSpace = 10000;
        $freeSpace = disk_free_space('/');
        $usedSpace = $totalSpace - $freeSpace;
        // Рассчитать процент использования
        $usagePercentage = ($usedSpace / $totalSpace) * 100;

        // Определить статус (true: только чтение, false: запись разрешена)
        $status = $usagePercentage >= 90;

        // Сохранить процентное соотношение и статус в кэш
        Cache::forever('disk_usage', [
            'percentage' => $usagePercentage,
            'status' => $status,
        ]);
//TODO: что-то
    }
}
