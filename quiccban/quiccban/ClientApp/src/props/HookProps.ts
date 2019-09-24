import { RouteComponentProps } from "react-router";
import { APIClient } from "../apiclient/APIClient";
import { ISelfUser } from "../entities/user/ISelfUser";

export interface HookProps extends RouteComponentProps {
    user?: ISelfUser;
    apiClient: APIClient;
}