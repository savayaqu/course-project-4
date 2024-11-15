<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ForbiddenException;
use App\Exceptions\Api\UnauthorizedException;
use Closure;
use Illuminate\Http\Request;

class CheckRole
{
    public function handle(Request $request, Closure $next, ...$allowedRoles)
    {
        $user = $request->user();

        if (!$user)
            throw new UnauthorizedException();

        $currentRole = $user->role->code;
        if (!in_array($currentRole, $allowedRoles))
            throw new ForbiddenException();

        $request->attributes->add(['role' => $currentRole]);
        return $next($request);
    }
}
