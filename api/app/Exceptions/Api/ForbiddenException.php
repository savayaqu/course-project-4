<?php

namespace App\Exceptions\Api;

use Illuminate\Http\Exceptions\HttpResponseException;


class ForbiddenException extends HttpResponseException
{
    public function __construct($class = null)
    {
        $message = 'Forbidden' . ($class ? ' to ' . class_basename($class) : '');
        parent::__construct(throw new ApiException($message, 403));
    }
}
