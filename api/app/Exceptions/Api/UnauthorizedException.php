<?php

namespace App\Exceptions\Api;

class UnauthorizedException extends ApiException
{
    public function __construct()
    {
        parent::__construct('Unauthorized', 401);
    }
}
