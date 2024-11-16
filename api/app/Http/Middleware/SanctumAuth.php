<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\ForbiddenException;
use App\Exceptions\Api\UnauthorizedException;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Laravel\Sanctum\Guard;
use Symfony\Component\HttpFoundation\Response;

class SanctumAuth
{
    public function handle(Request $request, Closure $next): Response
    {
        $user = Auth::guard('sanctum')->user();
        if (!$user) {
            throw new UnauthorizedException();
        }
        if ($user->is_banned) {
            throw new ApiException('Your account is banned.', 403);
        }

        return $next($request);
    }
}
