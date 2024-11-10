<?php
namespace App\Http\Middleware;

use Closure;
use Illuminate\Support\Facades\DB;

class LogDatabaseQueries
{
    public function handle($request, Closure $next)
    {
        // Включаем логирование запросов
        DB::enableQueryLog();

        // Продолжаем выполнение запроса
        $response = $next($request);

        // Получаем все запросы
        $queries = collect(DB::getQueryLog());

        // Выводим запросы
        foreach ($queries as $query) echo str_replace('?', $query['bindings'][0] ?? '?', $query['query']). ";\n\n";

        return $response;
      //return [
      //    ...$response->original,
      //    'queriesCount' => $queries->count(),
      //    'queries' => $queries->map(fn($query) => $query['query']),
      //    'queriesOrig' => $queries,
      //];
    }
}
