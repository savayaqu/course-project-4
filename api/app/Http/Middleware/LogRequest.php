<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Response;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class LogRequest
{
    public function handle(Request $request, Closure $next)
    {
        try {
            $method = strtoupper($request->getMethod());
            if (in_array($method, ['GET', 'PUT', 'PATCH', 'POST', 'DELETE']))
                DB::enableQueryLog();
        }
        catch (\Exception $e) {
            Log::error($e);
        }
        return $next($request);
    }

    public function terminate(Request $request, $response)
    {
        try {
            $method = strtoupper($request->getMethod());
            if (!in_array($method, ['GET', 'PUT', 'PATCH', 'POST', 'DELETE']))
                return;
            $method = str_pad($method, 6, ' ');

            $dbQueries = DB::getQueryLog();
            $dbQueryStrings = [];
            foreach ($dbQueries as $dbQuery) {
                $query = $dbQuery['query'];
                $bindings = $dbQuery['bindings'];

                // Заменяем вопросительные знаки на значения параметров
                foreach ($bindings as $binding)
                    $query = preg_replace('/\?/', "'" . addslashes($binding) . "'", $query, 1);

                $dbQueryStrings[] = $query . ";\n";
            }

            $code = $response->getStatusCode();
            $sign = $response->isRedirection()      ? "🔵" : (
                $response->isInformational()        ? "🟣" : (
                    $response->isSuccessful()       ? "🟢" : (
                        $response->isClientError()  ? "🟡" : "🔴"
                    )
                )
            );

            $uri = $request->getPathInfo();
            $origin = $request->headers->get('origin') ?? "NO_ORIGIN";
            $userId = $request->user()?->id ??
                ($request->has('sign')
                    ? explode('_', $request->sign)[0]
                    : "GUEST  ");

            if (is_numeric($userId))
                $userId = str_pad($userId, 7, '0', STR_PAD_LEFT);

            $reqQuery = $request->getQueryString();
            $message = "$sign $code $method 👤$userId $origin $uri"
                . ($reqQuery ? "?$reqQuery" : '')
                . (!empty($dbQueryStrings) ? "\n" : '')
                . implode('', $dbQueryStrings);

            Log::channel('http-request')->log('info', $message);
        }
        catch (\Exception $e) {
            Log::error($e);
        }
    }
}
