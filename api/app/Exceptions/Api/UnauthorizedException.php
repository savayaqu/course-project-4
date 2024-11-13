<?php

namespace App\Exceptions\Api;

use Illuminate\Http\Exceptions\HttpResponseException;


class UnauthorizedException extends HttpResponseException
{
    public function __construct()
    {
        parent::__construct(throw new ApiException('Unauthorized', 401));
    }
}
