<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ApiException;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Symfony\Component\HttpFoundation\Response;

class CheckRole
{

    public function handle(Request $request, Closure $next, ...$role)
    {
        if (Auth::check() &&  Auth::user()->codeRole($role)) {
            return $next($request);
        }
        throw new ApiException('Forbidden for you', '403');
    }
}
